using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SteamKit2.Blob
{
    class DictionarySerializer : IBlobSerializer
    {
        private IBlobSerializer from;
        private Type dictType;
        private Type leftType;
        private MethodInfo add;

        public Type ExpectedType
        {
            get
            {
                return dictType;
            }
        }

        public DictionarySerializer(Type type, Type leftType, Type innerType)
        {
            Type[] typeParams = { leftType, innerType };
            add = type.GetMethod("Add", typeParams);

            this.dictType = type;
            this.leftType = leftType;
            this.from = BlobTypedReader.GetSerializerForType(innerType);
        }

        public object Read(Object target, BlobReader reader)
        {
            object[] args = new object[2];

            using (reader = reader.ReadFieldBlob())
            {
                while (reader.CanTakeBytes(BlobReader.FieldHeaderLength))
                {
                    reader.ReadFieldHeader();

                    switch (Type.GetTypeCode(leftType))
                    {
                        case TypeCode.Int32:
                            args[0] = BitConverter.ToInt32(reader.ByteKey, 0);
                            break;
                        case TypeCode.String:
                            args[0] = Encoding.ASCII.GetString(reader.ByteKey, 0, reader.FieldKeyBytes);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    args[1] = from.Read(target, reader);
                    add.Invoke(target, args);
                }

                reader.SkipSpare();
            }

            return null;
        }


        public void EmitRead(JITContext context)
        {
            context.ReadFieldBlob();
            context.PushType(typeof(BlobReader));

            Label readField = context.CreateLabel();
            Label cleanup = context.CreateLabel();

            context.MarkLabel(readField);
            context.CanReadBytes(BlobReader.FieldHeaderLength);
            context.GotoWhenFalse(cleanup);

            context.ReadFieldHeader();

            context.LoadLocal(context.PeekTypeDef());

            switch (Type.GetTypeCode(leftType))
            {
                case TypeCode.Int32:
                    context.LoadByteKey();
                    context.LoadIntConstant(0);
                    context.BitConvertTo("Int32");
                    break;
                case TypeCode.String:
                    context.PushUTF8Encoding();
                    context.LoadByteKey();
                    context.LoadIntConstant(0);
                    context.GetFieldKeyBytes();
                    context.ByteConvertToString();
                    break;
                default:
                    throw new NotImplementedException();
            }

            from.EmitRead(context);
            context.EmitCall(add);
            context.Goto(readField);
           
            context.MarkLabel(cleanup);
            context.SkipSpare();

            context.DisposeReaderOnTop();
            context.PopType(typeof(BlobReader));
        }
    }
}
