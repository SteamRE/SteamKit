using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SteamKit2.Blob
{
    class ListSerializer : IBlobSerializer
    {
        private IBlobSerializer from;
        private MethodInfo add;
        private Type listType;

        public Type ExpectedType
        {
            get
            {
                return listType;
            }
        }

        public ListSerializer(Type type, Type innerType)
        {
            Type[] typeParams = {innerType};
            add = type.GetMethod("Add", typeParams);

            listType = type;
            from = BlobTypedReader.GetSerializerForType(innerType);
        }

        public object Read(Object target, BlobReader reader)
        {
            object[] args = new object[1];

            using (reader = reader.ReadFieldBlob())
            {
                while (reader.CanTakeBytes(BlobReader.FieldHeaderLength))
                {
                    reader.ReadFieldHeader();

                    // if there are no data bytes, we can "probably" assume that the keys are the values
                    if (reader.FieldDataBytes > 0)
                    {
                        args[0] = from.Read(target, reader);
                    }
                    else
                    {
                        // only handles ints, refactor this probably
                        args[0] = BitConverter.ToInt32(reader.ByteKey, 0);
                    }

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

            // is this a safe optimization?
            if (from.ExpectedType.IsValueType)
            {
                context.LoadByteKey();
                context.LoadIntConstant(0);
                context.BitConvertToInt32();
                context.EmitCall(add);
                context.Goto(readField);
            }
            else
            {
                from.EmitRead(context);
                context.EmitCall(add);
                context.Goto(readField);
            }

            context.MarkLabel(cleanup);
            context.SkipSpare();

            context.DisposeReaderOnTop();
            context.PopType(typeof(BlobReader));
        }
    }
}
