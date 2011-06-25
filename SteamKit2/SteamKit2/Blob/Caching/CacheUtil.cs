using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace SteamKit2
{
    public class CacheContext
    {
        public Dictionary<Type, FastPropertyInfo[]> FastPropCache { get; set; }
        public Dictionary<MemberInfo, object[]> MemberAttribMap { get; set; }
        public Dictionary<Type, InstantiateObjectHandler> ConstructCache { get; set; }

        public CacheContext()
        {
            FastPropCache = new Dictionary<Type, FastPropertyInfo[]>();
            MemberAttribMap = new Dictionary<MemberInfo, object[]>();
            ConstructCache = new Dictionary<Type, InstantiateObjectHandler>();
        }
    }

    public static class CacheUtil
    {
        public static FastPropertyInfo[] GetCachedPropertyInfo(this Type t, CacheContext context)
        {
            FastPropertyInfo[] fpropinfo;

            if (context.FastPropCache.TryGetValue(t, out fpropinfo))
                return fpropinfo;

            List<FastPropertyInfo> propGen = new List<FastPropertyInfo>();

            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                propGen.Add(new FastPropertyInfo(prop));
            }

            fpropinfo = propGen.ToArray();
            context.FastPropCache.Add(t, fpropinfo);
            return fpropinfo;
        }

        public static object[] GetCachedCustomAttribs(CacheContext context, MemberInfo mi, Type x)
        {
            object[] attribs;

            if (context.MemberAttribMap.TryGetValue(mi, out attribs))
                return attribs;

            attribs = mi.GetCustomAttributes(x, false);
            context.MemberAttribMap.Add(mi, attribs);

            return attribs;
        }

        public static T GetAttribute<T>(this MemberInfo mi, CacheContext context)
            where T : Attribute
        {
            T[] attribs = (T[])GetCachedCustomAttribs(context, mi, typeof(T));

            if (attribs == null || attribs.Length == 0)
                return null;

            return attribs[0];
        }

        public static object FastConstruct(CacheContext context, Type T)
        {
            InstantiateObjectHandler construct;

            if (context.ConstructCache.TryGetValue(T, out construct))
                return construct();

            construct = (InstantiateObjectHandler)DynamicMethodCompiler.CreateInstantiateObjectHandler(T);

            context.ConstructCache.Add(T, construct);
            return construct();
        }


        private delegate void InvokeAdd(object target, object value);
        private delegate void InvokeAdd2(object target, object key, object value);

        private static Dictionary<Type, InvokeAdd> AddCache = new Dictionary<Type, InvokeAdd>();
        private static Dictionary<Type, InvokeAdd2> Add2Cache = new Dictionary<Type, InvokeAdd2>();

        internal static void HandleAddCall(PropertyInfo prop, int genericCount, object target, object key, object value)
        {
            Type propType = prop.PropertyType;
            InvokeAdd addDelegate;
            InvokeAdd2 add2Delegate;

            if (AddCache.TryGetValue(propType, out addDelegate))
            {
                addDelegate(target, value);
                return;
            }
            else if (Add2Cache.TryGetValue(propType, out add2Delegate))
            {
                add2Delegate(target, key, value);
                return;
            }

            var methodInfo = prop.PropertyType.GetMethod("Add");

            if (genericCount == 1)
            {
                DynamicMethod dm = new DynamicMethod("InvokeAdd", null, new Type[] { typeof(object), typeof(object) }, typeof(BlobUtil).Module, true);
                ILGenerator ilgen = dm.GetILGenerator();

                Type generic = prop.PropertyType.GetGenericArguments()[0];

                ilgen.Emit(OpCodes.Ldarg_0);
                //ilgen.Emit(OpCodes.Castclass, prop.PropertyType);
                ilgen.Emit(OpCodes.Ldarg_1);
                if (generic.IsValueType)
                {
                    ilgen.Emit(OpCodes.Unbox_Any, generic);
                }
                //ilgen.Emit(OpCodes.Castclass, prop.PropertyType.GetGenericArguments()[0]);
                ilgen.EmitCall(OpCodes.Callvirt, methodInfo, null);
                ilgen.Emit(OpCodes.Ret);

                addDelegate = (InvokeAdd)dm.CreateDelegate(typeof(InvokeAdd));
                AddCache.Add(propType, addDelegate);

                addDelegate(target, value);
            }
            else
            {
                DynamicMethod dm = new DynamicMethod("InvokeAdd2", null, new Type[] { typeof(object), typeof(object), typeof(object) }, typeof(BlobUtil).Module, true);
                ILGenerator ilgen = dm.GetILGenerator();

                Type genericKey = prop.PropertyType.GetGenericArguments()[0];
                Type genericValue = prop.PropertyType.GetGenericArguments()[1];

                ilgen.Emit(OpCodes.Ldarg_0);
                //ilgen.Emit(OpCodes.Castclass, prop.PropertyType);
                ilgen.Emit(OpCodes.Ldarg_1);
                if (genericKey.IsValueType)
                {
                    ilgen.Emit(OpCodes.Unbox_Any, genericKey);
                }
                //ilgen.Emit(OpCodes.Castclass, prop.PropertyType.GetGenericArguments()[0]);
                ilgen.Emit(OpCodes.Ldarg_2);
                if (genericValue.IsValueType)
                {
                    ilgen.Emit(OpCodes.Unbox_Any, genericValue);
                }
                //ilgen.Emit(OpCodes.Castclass, prop.PropertyType.GetGenericArguments()[1]);
                ilgen.EmitCall(OpCodes.Callvirt, methodInfo, null);
                ilgen.Emit(OpCodes.Ret);

                add2Delegate = (InvokeAdd2)dm.CreateDelegate(typeof(InvokeAdd2));
                Add2Cache.Add(propType, add2Delegate);

                add2Delegate(target, key, value);
            }
        }
    }
}
