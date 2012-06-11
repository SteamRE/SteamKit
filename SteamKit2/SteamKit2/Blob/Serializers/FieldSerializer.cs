using System;
using System.Reflection;

namespace SteamKit2.Blob
{
    class FieldSerializer : IBlobSerializer
    {
        private IBlobSerializer from;
        private FieldInfo field;

        public Type ExpectedType
        {
            get
            {
                return typeof(FieldInfo);
            }
        }

        public FieldSerializer(FieldInfo field)
        {
            this.field = field;
            this.from = BlobTypedReader.GetSerializerForType(field.FieldType);
        }

        public object Read(Object target, BlobReader reader)
        {
            if (from is ListSerializer || from is DictionarySerializer)
            {
                object value = Activator.CreateInstance(field.FieldType);
                field.SetValue(target, value);
                from.Read(value, reader);
            }
            else
            {
                field.SetValue(target, from.Read(target, reader));
            }

            return null;
        }

        public void EmitRead(JITContext context)
        {
            context.LoadLocal(context.PeekTypeDef());

            if (from is ListSerializer || from is DictionarySerializer)
            {
                context.CreateType(field.FieldType);
                context.PushType(field.FieldType);
                context.PushTypeDef(context.PeekTop(field.FieldType));

                context.LoadLocal(context.PeekTop(field.FieldType));
                context.StoreField(field);

                from.EmitRead(context);
                context.PopTypeDef();
                context.PopType(field.FieldType);
            }
            else
            {
                from.EmitRead(context);
                context.StoreField(field);
            }
        }
    }
}
