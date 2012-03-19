using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace SteamKit2.Blob
{
    /// <summary>
    /// Provides caching for the fast construction of types
    /// </summary>
    public class CacheContext
    {
        internal Dictionary<Type, FastPropertyInfo[]> FastPropCache { get; set; }
        internal Dictionary<MemberInfo, object[]> MemberAttribMap { get; set; }
        internal Dictionary<Type, InstantiateObjectHandler> ConstructCache { get; set; }

        /// <summary>
        /// Initializes the default state of the CacheContext
        /// </summary>
        public CacheContext()
        {
            FastPropCache = new Dictionary<Type, FastPropertyInfo[]>();
            MemberAttribMap = new Dictionary<MemberInfo, object[]>();
            ConstructCache = new Dictionary<Type, InstantiateObjectHandler>();
        }
    }

    /// <summary>
    /// Cache utility functions to operate on a CacheContext
    /// </summary>
    public static class CacheUtil
    {
        /// <summary>
        /// Retrieve the cached property info for a class given the CacheContext
        /// </summary>
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

        /// <summary>
        /// Retrieve the cached list of custom attributes for a member
        /// </summary>
        public static object[] GetCachedCustomAttribs(CacheContext context, MemberInfo mi, Type x)
        {
            object[] attribs;

            if (context.MemberAttribMap.TryGetValue(mi, out attribs))
                return attribs;

            attribs = mi.GetCustomAttributes(x, false);
            context.MemberAttribMap.Add(mi, attribs);

            return attribs;
        }

        /// <summary>
        /// Retrieve the cached attribute for a member 
        /// </summary>
        public static T GetAttribute<T>(this MemberInfo mi, CacheContext context)
            where T : Attribute
        {
            T[] attribs = (T[])GetCachedCustomAttribs(context, mi, typeof(T));

            if (attribs == null || attribs.Length == 0)
                return null;

            return attribs[0];
        }

        /// <summary>
        /// Construct a type using the cached constructor
        /// </summary>
        /// todo: consider pooling
        public static object FastConstruct(CacheContext context, Type T)
        {
            InstantiateObjectHandler construct;

            if (context.ConstructCache.TryGetValue(T, out construct))
                return construct();

            construct = (InstantiateObjectHandler)DynamicMethodCompiler.CreateInstantiateObjectHandler(T);

            context.ConstructCache.Add(T, construct);
            return construct();
        }

        /// <summary>
        /// Helper method to test if a type is a list or dictionary
        /// </summary>
        public static bool IsTypeListOrDictionary(this Type type, out Type wantType, out int count)
        {
            if (!type.IsGenericType)
            {
                count = 0;
                wantType = null;
                return false;
            }

            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                wantType = type.GetGenericArguments()[0];
                count = 1;
                return true;
            }
            else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                wantType = type.GetGenericArguments()[1];
                count = 2;
                return true;
            }

            count = 0;
            wantType = null;
            return false;
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
