using System;
using System.Reflection;

namespace SteamKit2.Blob
{
    class PropertySerializer : IBlobSerializer
    {
        private IBlobSerializer from;
        private PropertyInfo prop;

        public Type ExpectedType
        {
            get
            {
                return typeof(PropertyInfo);
            }
        }

        public PropertySerializer(PropertyInfo prop)
        {
            this.prop = prop;
            this.from = BlobTypedReader.GetSerializerForType(prop.PropertyType);
        }

        public object Read(Object target, BlobReader reader)
        {
            if (from is ListSerializer || from is DictionarySerializer)
            {
                object value = Activator.CreateInstance(prop.PropertyType);
                prop.SetValue(target, value, null);
                from.Read(value, reader);
            }
            else
            {
                prop.SetValue(target, from.Read(target, reader), null);
            }

            return null;
        }

        public void EmitRead(JITContext context)
        {
            context.LoadLocal(context.PeekTypeDef());

            if (from is ListSerializer || from is DictionarySerializer)
            {
                context.CreateType(prop.PropertyType);
                context.PushType(prop.PropertyType);
                context.PushTypeDef(context.PeekTop(prop.PropertyType));

                context.LoadLocal(context.PeekTop(prop.PropertyType));
                context.StoreProp(prop);

                from.EmitRead(context);
                context.PopTypeDef();
                context.PopType(prop.PropertyType);
            }
            else
            {
                from.EmitRead(context);
                context.StoreProp(prop);
            }
        }
    }
}