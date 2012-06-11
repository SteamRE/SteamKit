using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SteamKit2.Blob
{
    class TypeSerializer : IBlobSerializer
    {
        private Type targetType;
        private BlobFieldAttribute[] fields;

        public Type ExpectedType
        {
            get
            {
                return targetType;
            }
        }

        public TypeSerializer(Type targetType)
        {
            var fields = new List<BlobFieldAttribute>();
            this.targetType = targetType;

            foreach (MemberInfo member in targetType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                BlobFieldAttribute[] attribs = (BlobFieldAttribute[])member.GetCustomAttributes(typeof(BlobFieldAttribute), false);
                if (attribs.Length == 0) continue;

                BlobFieldAttribute attrib = attribs[0];
                FieldInfo field = (FieldInfo)member;

                attrib.Serializer = new FieldSerializer(field);

                fields.Add(attrib);
            }

            this.fields = fields.ToArray();
        }

        public object Read(Object target, BlobReader reader)
        {
            if(target != null)
                reader = reader.ReadFieldBlob();

            object result = Activator.CreateInstance(targetType);

            read_field:
            while (reader.CanTakeBytes(BlobReader.FieldHeaderLength))
            {
                reader.ReadFieldHeader();

                for(int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];

                    if (field.ByteKey.Length == reader.FieldKeyBytes && BlobUtil.UnsafeCompare(field.ByteKey, reader.ByteKey))
                    {
                        field.Serializer.Read(result, reader);
                        goto read_field;
                    }
                }

                reader.SkipField();
            }

            reader.SkipSpare();

            if (target != null)
                reader.Dispose();

            return result;
        }

        public void EmitRead(JITContext context)
        {
            LocalBuilder top = context.PeekTypeDef();
            if (top != null)
            {
                context.ReadFieldBlob();
                context.PushType(typeof(BlobReader));
            }

            context.CreateType(targetType);
            context.PushType(targetType);
            context.PushTypeDef(context.PeekTop(targetType));

            Label readField = context.CreateLabel();
            Label cleanup = context.CreateLabel();

            context.MarkLabel(readField);
            context.CanReadBytes(BlobReader.FieldHeaderLength);
            context.GotoWhenFalse(cleanup);

            context.ReadFieldHeader();

            for (int i = 0; i < fields.Length; i++)
            {
                Label nextField = context.CreateLabel();

                context.EmitKeyTest(fields[i].ByteKey, nextField); //context.TestKey(); context.GotoWhenFalse(nextField);

                fields[i].Serializer.EmitRead(context);

                context.Goto(readField);
                context.MarkLabel(nextField);
            }
            context.SkipField();
            context.Goto(readField);

            context.MarkLabel(cleanup);
            context.SkipSpare();

            if (top != null)
            {
                context.DisposeReaderOnTop();
                context.PopType(typeof(BlobReader));
            }

            context.LoadLocal(context.PeekTypeDef());
            context.PopTypeDef();
            context.PopType(targetType);
        }
    }
}
