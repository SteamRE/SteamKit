using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace SteamKit2.Blob
{
    internal class JITContext
    {
        public delegate object BlobDeserializer(BlobReader source, byte[][] keyList);

        private DynamicMethod method;
        private ILGenerator ilgen;
        private int next;

        private class TypeLocalPool
        {
            private Stack<LocalBuilder> stack;
            private Queue<LocalBuilder> queue;
            private ILGenerator ilgen;

            public Type Type;

            public TypeLocalPool(ILGenerator gen, Type t)
            {
                this.Type = t;
                this.stack = new Stack<LocalBuilder>();
                this.queue = new Queue<LocalBuilder>();
                this.ilgen = gen;
            }

            public LocalBuilder Top
            {
                get
                {
                    return stack.Count == 0 ? null : stack.Peek();
                }
            }

            public LocalBuilder Get()
            {
                LocalBuilder local;
                if (queue.Count > 0)
                    local = queue.Dequeue();
                else
                    local = ilgen.DeclareLocal(Type);
                stack.Push(local);
                return local;
            }

            public void Return()
            {
                queue.Enqueue(stack.Pop());
            }
        }

        private Dictionary<Type, TypeLocalPool> typePool;
        private Stack<LocalBuilder> genericTypeStack;

        public static List<byte[]> ByteKeys = new List<byte[]>();

        public JITContext(Type source)
        {
            Type[] paramTypes;
            Type returnType;

            returnType = typeof(object);
            paramTypes = new Type[] { typeof(BlobReader), typeof(byte[][]) };

            method = new DynamicMethod("blob_" + Interlocked.Increment(ref next), returnType, paramTypes, source, true);
            ilgen = method.GetILGenerator();

            typePool = new Dictionary<Type, TypeLocalPool>();
            genericTypeStack = new Stack<LocalBuilder>();
        }

        public LocalBuilder PeekTop(Type t)
        {
            if (!typePool.ContainsKey(t))
                typePool[t] = new TypeLocalPool(ilgen, t);

            return typePool[t].Top;
        }

        public void PushType(Type t)
        {
            if (!typePool.ContainsKey(t))
                typePool[t] = new TypeLocalPool(ilgen, t);

            var local = typePool[t].Get();
            StoreLocal(local);
        }

        public void PopType(Type t)
        {
            typePool[t].Return();
        }

        public void PushTypeDef(LocalBuilder local)
        {
            genericTypeStack.Push(local);
        }

        public LocalBuilder PeekTypeDef()
        {
            return genericTypeStack.Count > 0 ? genericTypeStack.Peek() : null;
        }

        public void PopTypeDef()
        {
            genericTypeStack.Pop();
        }

        public void StoreLocal(LocalBuilder local)
        {
            switch (local.LocalIndex)
            {
                case 0: ilgen.Emit(OpCodes.Stloc_0); break;
                case 1: ilgen.Emit(OpCodes.Stloc_1); break;
                case 2: ilgen.Emit(OpCodes.Stloc_2); break;
                case 3: ilgen.Emit(OpCodes.Stloc_3); break;
                default:
                    OpCode code = (local.LocalIndex < 256) ? OpCodes.Stloc_S : OpCodes.Stloc;
                    ilgen.Emit(code, local.LocalIndex);
                    break;
            }
        }

        public void LoadLocal(LocalBuilder local)
        {
            switch (local.LocalIndex)
            {
                case 0: ilgen.Emit(OpCodes.Ldloc_0); break;
                case 1: ilgen.Emit(OpCodes.Ldloc_1); break;
                case 2: ilgen.Emit(OpCodes.Ldloc_2); break;
                case 3: ilgen.Emit(OpCodes.Ldloc_3); break;
                default:
                    OpCode code = (local.LocalIndex < 256) ? OpCodes.Ldloc_S : OpCodes.Ldloc;
                    ilgen.Emit(code, local.LocalIndex);
                    break;
            }
        }

        public LocalBuilder CreateLocal(Type t)
        {
            return ilgen.DeclareLocal(t);
        }

        public void StoreField(FieldInfo field)
        {
            ilgen.Emit(OpCodes.Stfld, field);
        }

        public void StoreProp(PropertyInfo prop)
        {
            EmitCall(prop.GetSetMethod());
        }

        public void EmitCall(MethodInfo method)
        {
            OpCode opcode = (method.IsStatic || method.DeclaringType.IsValueType) ? OpCodes.Call : OpCodes.Callvirt;
            ilgen.EmitCall(opcode, method, null);
        }

        static Type[] EmptyParams = new Type[] { };

        public void CreateType(Type newType)
        {
            CreateType(newType, EmptyParams);
        }

        public void CreateType(Type newType, Type[] paramType)
        {
            if (newType.IsValueType)
            {
                ilgen.Emit(OpCodes.Initobj, newType);
            }
            else
            {
                ConstructorInfo ctor = newType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, paramType, null);
                if (ctor == null) throw new InvalidOperationException("Cannot construct " + newType);
                ilgen.Emit(OpCodes.Newobj, ctor);
            }
        }

        public Label CreateLabel()
        {
            return ilgen.DefineLabel();
        }

        public void MarkLabel(Label label)
        {
            ilgen.MarkLabel(label);
        }

        public void Goto(Label label)
        {
            ilgen.Emit(OpCodes.Br, label);
        }

        public void GotoWhenFalse(Label label)
        {
            ilgen.Emit(OpCodes.Brfalse, label);
        }

        public void GotoLessThanEqualZero(Label label)
        {
            ilgen.Emit(OpCodes.Ldc_I4_0);
            ilgen.Emit(OpCodes.Ble, label);
        }

        public void LoadBlobReader()
        {
            var top = PeekTop(typeof(BlobReader));
            if (top == null)
                ilgen.Emit(OpCodes.Ldarg_0);
            else
                LoadLocal(top);
        }

        private MethodInfo GetReaderMethod(string methodName)
        {
            MethodInfo method = typeof(BlobReader).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new ArgumentException("methodName");

            return method;
        }

        public void BitConvertToInt32()
        {
            MethodInfo convMethod = typeof(BitConverter).GetMethod("ToInt32", BindingFlags.Public | BindingFlags.Static);
            if (convMethod == null) throw new ArgumentException("methodName");

            EmitCall(convMethod);
        }

        public void PushUTF8Encoding()
        {
            MethodInfo getMethod = typeof(Encoding).GetMethod("get_UTF8", BindingFlags.Public | BindingFlags.Static);
            if (getMethod == null) throw new ArgumentException("methodName");

            EmitCall(getMethod);
        }

        public void ByteConvertToString()
        {
            MethodInfo convMethod = typeof(Encoding).GetMethod("GetString", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(byte[]), typeof(int), typeof(int) }, null);
            if (convMethod == null) throw new ArgumentException("methodName");

            EmitCall(convMethod);
        }

        public void EmitStreamCall(string name)
        {
            MethodInfo method = typeof(Stream).GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) method = typeof(StreamHelpers).GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (method == null) throw new ArgumentException("methodName");

            EmitCall(method);
        }

        public void PushByteCompare()
        {
            MethodInfo compMethod = typeof(BlobUtil).GetMethod("UnsafeCompare", BindingFlags.Public | BindingFlags.Static);
            if (compMethod == null) throw new ArgumentException("methodName");

            EmitCall(compMethod);
        }

        public void CompareGreaterThanZero()
        {
            ilgen.Emit(OpCodes.Ldc_I4_0);
            ilgen.Emit(OpCodes.Cgt);
        }

        public void EmitKeyTest(byte[] key, Label failed)
        {
            LoadByteKey(key);
            ilgen.Emit(OpCodes.Ldlen);
            ilgen.Emit(OpCodes.Conv_I4);
            GetFieldKeyBytes();
            ilgen.Emit(OpCodes.Bne_Un, failed);
            LoadByteKey(key);
            LoadByteKey();
            PushByteCompare();
            GotoWhenFalse(failed);
        }

        public void EmitKeyPeekTest(int key, Label failed)
        {
            LoadIntConstant(key);
            LoadPeekKey();
            ilgen.Emit(OpCodes.Bne_Un, failed);
        }

        public void LoadIntConstant(int value)
        {
            switch (value)
            {
                case 0: ilgen.Emit(OpCodes.Ldc_I4_0); break;
                case 1: ilgen.Emit(OpCodes.Ldc_I4_1); break;
                case 2: ilgen.Emit(OpCodes.Ldc_I4_2); break;
                case 3: ilgen.Emit(OpCodes.Ldc_I4_3); break;
                default:
                    ilgen.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        public void Subtract()
        {
            ilgen.Emit(OpCodes.Sub);
        }

        public void LoadByteKey(byte[] key)
        {
            int index = ByteKeys.FindIndex(x => x.Length == key.Length && BlobUtil.UnsafeCompare(x, key));
            if (index < 0)
            {
                ByteKeys.Add(key);
                index = ByteKeys.Count - 1;
            }

            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(index < 256 ? OpCodes.Ldc_I4_S : OpCodes.Ldc_I4, index);
            ilgen.Emit(OpCodes.Ldelem, typeof(byte[]));
        }

        public void ReadFieldBlob()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("ReadFieldBlob"));
        }

        public void ReadFieldStream()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("ReadFieldStream"));
        }

        public void ReadFieldHeader()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("ReadFieldHeader"));
        }


        public void CanReadBytes(int count)
        {
            LoadBlobReader();
            ilgen.Emit(OpCodes.Ldc_I4, count);
            EmitCall(GetReaderMethod("CanTakeBytes"));
        }

        public void GetFieldDataBytes()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("get_FieldDataBytes"));
        }

        public void GetFieldKeyBytes()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("get_FieldKeyBytes"));
        }

        public void LoadPeekKey()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("get_PeekIntKey"));
        }

        public void LoadByteKey()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("get_ByteKey"));
        }

        public void SkipField()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("SkipField"));
        }

        public void SkipSpare()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("SkipSpare"));
        }

        public void DisposeReaderOnTop()
        {
            LoadBlobReader();
            EmitCall(GetReaderMethod("Dispose"));
        }

        private void Return()
        {
            ilgen.Emit(OpCodes.Castclass, typeof(object));
            ilgen.Emit(OpCodes.Ret);
        }

        public static BlobDeserializer BuildDeserializer(IBlobSerializer source)
        {
            JITContext ctx = new JITContext(source.ExpectedType);

            source.EmitRead(ctx);
            ctx.Return();

            return (BlobDeserializer)ctx.method.CreateDelegate(typeof(BlobDeserializer));
        }
    }
}
