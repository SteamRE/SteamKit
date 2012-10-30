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

        public void Pop()
        {
            ilgen.Emit(OpCodes.Pop);
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

        public void CreateArray(Type t)
        {
            ilgen.Emit(OpCodes.Newarr, t);
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

        public void GotoNotEqual(Label label)
        {
            ilgen.Emit(OpCodes.Bne_Un, label);
        }

        public void GotoGtOrEqual(Label label)
        {
            ilgen.Emit(OpCodes.Bge_Un, label);
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

        public MethodInfo GetReaderMethod(string methodName)
        {
            MethodInfo method = typeof(BlobReader).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new ArgumentException("methodName");

            return method;
        }

        public void BitConvertTo(String type)
        {
            MethodInfo convMethod = typeof(BitConverter).GetMethod("To" + type, BindingFlags.Public | BindingFlags.Static);
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

        public void CompareEqualTo(int value)
        {
            ilgen.Emit(OpCodes.Ldc_I4, value);
            ilgen.Emit(OpCodes.Ceq);
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
                case -1: ilgen.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: ilgen.Emit(OpCodes.Ldc_I4_0); break;
                case 1: ilgen.Emit(OpCodes.Ldc_I4_1); break;
                case 2: ilgen.Emit(OpCodes.Ldc_I4_2); break;
                case 3: ilgen.Emit(OpCodes.Ldc_I4_3); break;
                case 4: ilgen.Emit(OpCodes.Ldc_I4_4); break;
                case 5: ilgen.Emit(OpCodes.Ldc_I4_5); break;
                case 6: ilgen.Emit(OpCodes.Ldc_I4_6); break;
                case 7: ilgen.Emit(OpCodes.Ldc_I4_7); break;
                case 8: ilgen.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    ilgen.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        public void Add()
        {
            ilgen.Emit(OpCodes.Add);
        }

        public void Subtract()
        {
            ilgen.Emit(OpCodes.Sub);
        }

        public void Length()
        {
            ilgen.Emit(OpCodes.Ldlen);
            ilgen.Emit(OpCodes.Conv_I4);
        }

        public void Switch(Label[] table)
        {
            ilgen.Emit(OpCodes.Switch, table);
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
            LoadBlobField("source");
        }

        public void CanReadBytes(int count)
        {
            ilgen.Emit(OpCodes.Ldc_I4, count);
            LoadBlobReader();
            LoadBlobField("bytesAvailable");
            ilgen.Emit(OpCodes.Clt);
        }

        public void GetFieldDataBytes()
        {
            LoadBlobReader();
            LoadBlobField("dataBytes");
        }

        public void GetFieldKeyBytes()
        {
            LoadBlobReader();
            LoadBlobField("keyBytes");
        }

        public void LoadPeekKey()
        {
            LoadBlobReader();
            LoadBlobField("keyInt");
        }

        public void LoadByteKey()
        {
            LoadBlobReader();
            LoadBlobField("keyBuffer");
        }

        public void SkipField()
        {
            LoadBlobReader();
            LoadBlobField("source");
            LoadBlobReader();
            LoadBlobField("dataBytes");
            EmitStreamCall("ReadAndDiscard");
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


        public void LoadField(Type type, string field)
        {
            ilgen.Emit(OpCodes.Ldfld, type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
        }

        public void LoadStaticField(Type type, string field)
        {
            ilgen.Emit(OpCodes.Ldsfld, type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
        }

        public void LoadBlobField(string field)
        {
            LoadField(typeof(BlobReader), field);
        }

        public void StoreBlobField(string field)
        {
            ilgen.Emit(OpCodes.Stfld, typeof(BlobReader).GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
        }


        public static BlobDeserializer BuildDeserializer(IBlobSerializer source)
        {
            JITContext ctx = new JITContext(source.ExpectedType);

            source.EmitRead(ctx);
            ctx.Return();

            return (BlobDeserializer)ctx.method.CreateDelegate(typeof(BlobDeserializer));
        }

        public void StreamRead(int length, string type)
        {
            LoadStaticField(typeof(StreamHelpers), "data");
            LoadIntConstant(0);
            LoadIntConstant(length);
            EmitStreamCall("Read");
            Pop();
            LoadStaticField(typeof(StreamHelpers), "data");
            LoadIntConstant(0);
            BitConvertTo(type);
        }

        public void ReadFieldHeader()
        {
            var totalBytes = CreateLocal(typeof(Int32));

            LoadBlobReader();
            LoadBlobReader();
            LoadBlobReader();
            LoadBlobReader();
            LoadBlobReader();
            LoadBlobReader();
            LoadBlobReader();

            LoadBlobField("source");
            StreamRead(2, "UInt16");
            StoreBlobField("keyBytes");

            LoadBlobField("source");
            StreamRead(4, "Int32");
            StoreBlobField("dataBytes");

            LoadBlobField("bytesAvailable");
            LoadIntConstant(BlobReader.FieldHeaderLength);
            Subtract();
            StoreBlobField("bytesAvailable");

            var continueRead = CreateLabel();

            LoadBlobReader();
            LoadBlobField("keyBuffer");
            Length();
            LoadBlobReader();
            LoadBlobField("keyBytes");
            GotoGtOrEqual(continueRead);

            LoadBlobReader();
            LoadBlobReader();
            LoadBlobField("keyBytes");
            CreateArray(typeof(byte));
            StoreBlobField("keyBuffer");

            MarkLabel(continueRead);

            LoadBlobField("dataBytes");
            LoadBlobReader();
            LoadBlobField("keyBytes");
            Add();
            StoreLocal(totalBytes);

            LoadBlobReader();
            LoadBlobField("source");
            LoadBlobReader();
            LoadBlobField("keyBuffer");
            LoadIntConstant(0);
            LoadBlobReader();
            LoadBlobField("keyBytes");
            EmitStreamCall("Read");
            Pop();


            var cleanup = CreateLabel();
            var neg1 = CreateLabel();

            LoadBlobReader();
            LoadBlobField("keyBytes");
            LoadIntConstant(4);
            GotoNotEqual(neg1);

            LoadBlobReader();
            LoadBlobReader();
            LoadBlobField("keyBuffer");
            LoadIntConstant(0);
            BitConvertTo("Int32");
            StoreBlobField("keyInt");

            Goto(cleanup);

            MarkLabel(neg1);
            LoadBlobReader();
            LoadIntConstant(-1);
            StoreBlobField("keyInt");

            MarkLabel(cleanup);

            LoadBlobReader();
            LoadBlobReader();
            LoadBlobField("bytesAvailable");
            LoadLocal(totalBytes);
            Subtract();
            StoreBlobField("bytesAvailable");
        }
    }
}
