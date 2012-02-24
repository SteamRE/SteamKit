using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;
using SteamKit2;

namespace SteamKit2.Blob
{
    /// <summary>
    /// This attribute is used to mark properties, classes, and structs for typed reading.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | 
                    AttributeTargets.Class |
                    AttributeTargets.Struct,
                    Inherited = false, AllowMultiple = false)]
    public class BlobFieldAttribute : Attribute
    {
        /// <summary>
        /// Depth from stack.
        /// </summary>
        public int Depth { get; set; }
        /// <summary>
        /// Field key.
        /// </summary>
        public string FieldKey { get; set; }
        /// <summary>
        /// Depth to sub field for value.
        /// </summary>
        public int SubFieldDepth { get; set; }
        /// <summary>
        /// Value sub field to use for fields with blobs.
        /// </summary>
        public string SubFieldKey { get; set; }
        /// <summary>
        /// If complex, the sub type will be searched for blob fields.
        /// </summary>
        public bool Complex { get; set; }
    }

    /// <summary>
    /// Utility blob reader which processes blob data into the given type.
    /// </summary>
    /// <typeparam name="T">The type to process the blob into.</typeparam>
    public class BlobTypedReader<T> : BlobReader
    {
        private enum EProcessingState
        {
            TopSearch, // searching for top level field
            FieldSearch, // searching for any fields that match
            SubFieldSearch, // searching for a single sub field to match
            EveryFieldSearch, // match all fields, for List and Dictionary
            Invalid
        }

        private struct ProcessState
        {
            public int Depth;
            public EProcessingState State;
            public List<BlobFieldAttribute> Attributes;
            public List<PropertyInfo> Properties;

            public object WorkingType;

            public ProcessState(EProcessingState state, int depth, object target)
            {
                Attributes = new List<BlobFieldAttribute>();
                Properties = new List<PropertyInfo>();
                State = state;
                Depth = depth;
                WorkingType = target;
            }
        }

        /// <summary>
        /// Gets the completed type object available after processing.
        /// </summary>
        public T Target { get; private set; }

        private int depth;
        private int expectingIndex;
        private string expectedKey;
        private Stack<ProcessState> processStack;

        private static CacheContext TypedCache = new CacheContext();

        private void TypeStartBlob(EAutoPreprocessCode processCode, ECacheState cacheState)
        {
            if (expectingIndex >= 0)
            {
                throw new Exception("Could not parse type, expecting FieldValue but got Blob");
            }

            depth++;
        }

        private void TypeEndBlob()
        {
            depth--;
        }

        private void TypeStartField(FieldKeyType type, byte[] key, int fieldSize)
        {
            string keyValue;

            if (BlobUtil.IsIntDescriptor(key))
                keyValue = Convert.ToString(BitConverter.ToUInt32(key, 0));
            else
                keyValue = Encoding.UTF8.GetString(key);

            int fieldIndex;
            ProcessState top = PeekStackTop();

            if (top.State == EProcessingState.TopSearch &&
                TestFieldAttributeState(top, keyValue, out fieldIndex))
            {
                PushTypeAttributesToStack(typeof(T), Target);
            }
            else if (top.State == EProcessingState.FieldSearch &&
                    TestFieldAttributeState(top, keyValue, out fieldIndex))
            {
                PropertyInfo prop = top.Properties[fieldIndex];
                BlobFieldAttribute attrib = top.Attributes[fieldIndex];

                Type subType = prop.PropertyType;
                Type genericType;
                int genericCount;

                if(subType.IsTypeListOrDictionary(out genericType, out genericCount))
                {
                    object subInstance = CacheUtil.FastConstruct(TypedCache, subType); //Activator.CreateInstance(subType);

                    prop.SetValue(top.WorkingType, subInstance, null);

                    PushListTypeAttributesToStack(subInstance, attrib, prop);
                }
                else if (attrib.Complex == true)
                {
                    object subInstance = CacheUtil.FastConstruct(TypedCache, subType); //Activator.CreateInstance(subType);

                    prop.SetValue(top.WorkingType, subInstance, null);

                    PushTypeAttributesToStack(subType, subInstance);
                }
                else if (attrib.SubFieldDepth > 0 ||
                        attrib.SubFieldKey != null)
                {
                    PushSubSearchAttributeToStack(top, attrib, prop, top.WorkingType);
                }
                else
                {
                    expectingIndex = fieldIndex;
                }
            }
            else if (top.State == EProcessingState.SubFieldSearch &&
                    TestSubFieldAttributeState(top, keyValue, out fieldIndex))
            {
                expectingIndex = fieldIndex;
            }
            else if (top.State == EProcessingState.EveryFieldSearch &&
                    depth - top.Depth == 1)
            {
                PropertyInfo prop = top.Properties[0];

                Type subType;
                int genericCount;

                bool genericType = prop.PropertyType.IsTypeListOrDictionary(out subType, out genericCount);

                if (top.Attributes[0].Complex == true && genericType)
                {
                    object subInstance = CacheUtil.FastConstruct(TypedCache, subType); //Activator.CreateInstance(subType);

                    CacheUtil.HandleAddCall(prop, genericCount, top.WorkingType, keyValue, subInstance);

                    PushTypeAttributesToStack(subType, subInstance);
                }
                else if (fieldSize == 0 && genericType)
                {
                    // key is the actual field.
                    object value = GetDataForProp(key, subType);

                    CacheUtil.HandleAddCall(prop, genericCount, top.WorkingType, expectedKey, value);
                }
                else
                {
                    expectedKey = keyValue;
                    expectingIndex = 0;
                }
            }

        }

        private void TypeEndField()
        {
            // pop off fields for this depth when we're done with the blob
            if (PeekStackTop().Depth == depth)
            {
                PopTypeAttributesFromStack();
            }
        }

        private void TypeFieldValue(byte[] data)
        {
            if (expectingIndex >= 0)
            {
                ProcessState top = PeekStackTop();

                PropertyInfo prop = top.Properties[expectingIndex];

                if (top.State == EProcessingState.EveryFieldSearch)
                {
                    Type subType;
                    int genericCount;

                    if (prop.PropertyType.IsTypeListOrDictionary(out subType, out genericCount))
                    {
                        object value = GetDataForProp(data, subType);

                        CacheUtil.HandleAddCall(prop, genericCount, top.WorkingType, expectedKey, value);
                    }
                }
                else
                {
                    object value = GetDataForProp(data, prop.PropertyType);
                    prop.SetValue(top.WorkingType, value, null);
                }

                expectingIndex = -1;
            }
        }

        private BlobTypedReader(Stream input)
            : base(input)
        {
            Target = Activator.CreateInstance<T>();

            depth = 0;
            expectingIndex = -1;
            expectedKey = null;
            processStack = new Stack<ProcessState>();

            BuildParserState();

            Blob += TypeStartBlob;
            EndBlob += TypeEndBlob;
            Field += TypeStartField;
            EndField += TypeEndField;
            FieldValue += TypeFieldValue;
        }

        private void BuildParserState()
        {
            Type type = typeof(T);

            BlobFieldAttribute topLevelAttribute = type.GetAttribute<BlobFieldAttribute>(TypedCache);

            if (topLevelAttribute != null)
            {
                PushTopSearchAttributeToStack(topLevelAttribute);
            }
            else
            {
                PushTypeAttributesToStack(type, Target);
            }
        }

        private ProcessState PeekStackTop()
        {
            return processStack.Peek();
        }

        private bool TestFieldAttributeState(ProcessState top, string fieldKey, out int index)
        {
            List<BlobFieldAttribute> attributes = top.Attributes;

            for(int i = 0; i < attributes.Count; i++)
            {
                BlobFieldAttribute attrib = attributes[i];

                if (attrib.FieldKey == fieldKey &&
                    (attrib.Depth == 0 || depth - top.Depth == attrib.Depth))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private bool TestSubFieldAttributeState(ProcessState top, string fieldKey, out int index)
        {
            List<BlobFieldAttribute> attributes = top.Attributes;

            for (int i = 0; i < attributes.Count; i++)
            {
                BlobFieldAttribute attrib = attributes[i];

                if (attrib.SubFieldKey == fieldKey &&
                    (attrib.SubFieldDepth == 0 || depth - top.Depth == attrib.Depth))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }


        private void PushTypeAttributesToStack(Type t, object target)
        {
            ProcessState state = new ProcessState(EProcessingState.FieldSearch, depth, target);

            foreach (var field in t.GetCachedPropertyInfo(TypedCache))
            {
                BlobFieldAttribute fattrib = field.GetAttribute<BlobFieldAttribute>(TypedCache);

                if (fattrib == null)
                    continue;
                
                state.Attributes.Add(fattrib);
                state.Properties.Add(field);
            }

            processStack.Push(state);
        }

        private void PushListTypeAttributesToStack(object target, BlobFieldAttribute absorb, PropertyInfo prop)
        {
            ProcessState state = new ProcessState(EProcessingState.EveryFieldSearch, depth, target);

            state.Attributes.Add(absorb);
            state.Properties.Add(prop);

            processStack.Push(state);
        }

        private void PushTopSearchAttributeToStack(BlobFieldAttribute attrib)
        {
            ProcessState state = new ProcessState(EProcessingState.TopSearch, depth, Target);
            state.Attributes.Add(attrib);

            processStack.Push(state);
        }

        private void PushSubSearchAttributeToStack(ProcessState top, BlobFieldAttribute attrib, PropertyInfo prop, object target)
        {
            ProcessState state = new ProcessState(EProcessingState.SubFieldSearch, depth, target);
            state.Attributes.Add(attrib);
            state.Properties.Add(prop);

            state.WorkingType = top.WorkingType;

            processStack.Push(state);
        }

        private void PopTypeAttributesFromStack()
        {
            processStack.Pop();
        }


        private object GetDataForProp(byte[] buffer, Type propType)
        {
            object data = null;
            object integerData = null;

            Type enumType = null;

            // pull out integer data before parsing it, not always the # of bytes we want
            switch (buffer.Length)
            {
                case 1:
                    integerData = buffer[0];
                    break;
                case 2:
                    integerData = BitConverter.ToInt16(buffer, 0);
                    break;
                case 4:
                    integerData = BitConverter.ToInt32(buffer, 0);
                    break;
                case 8:
                    integerData = BitConverter.ToInt64(buffer, 0);
                    break;
            }

            if (propType.IsEnum)
            {
                enumType = propType;
                propType = Enum.GetUnderlyingType(propType);
            }

            if (propType == typeof(uint))
            {
                data = Convert.ToUInt32(integerData);
            }
            else if (propType == typeof(int))
            {
                data = Convert.ToInt32(integerData);
            }
            else if (propType == typeof(ushort))
            {
                data = Convert.ToUInt16(integerData);
            }
            else if (propType == typeof(short))
            {
                data = Convert.ToInt16(integerData);
            }
            else if (propType == typeof(string))
            {
                data = BlobUtil.TrimNull(Encoding.ASCII.GetString(buffer));
            }
            else if (propType == typeof(bool))
            {
                data = Convert.ToBoolean(integerData);
            }
            else if (propType == typeof(byte))
            {
                data = Convert.ToByte(integerData);
            }
            else if (propType == typeof(byte[]))
            {
                data = buffer;
            }
            else if (propType == typeof(ulong))
            {
                data = Convert.ToUInt64(integerData);
            }
            else if (propType == typeof(long))
            {
                data = Convert.ToInt64(integerData);
            }
            else
            {
                throw new NotImplementedException("Missing handler in GetDataForProp of type " + propType.ToString());
            }

            if (enumType != null)
            {
                data = Enum.ToObject(enumType, data);
            }

            return data;
        }


        /// <summary>
        /// Creates a new instance of a <see cref="BlobTypedReader&lt;T&gt;"/> for the given input stream.
        /// </summary>
        /// <param name="inputStream">The input stream to process.</param>
        /// <returns>A new <see cref="BlobReader"/> instance.</returns>
        public static new BlobTypedReader<T> Create(Stream inputStream)
        {
            return new BlobTypedReader<T>(inputStream);
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BlobTypedReader&lt;T&gt;"/> for the given input file.
        /// </summary>
        /// <param name="fileName">The input file to process.</param>
        /// <returns>A new <see cref="BlobReader"/> instance.</returns>
        public static new BlobTypedReader<T> Create(string fileName)
        {
            return Create(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 0x1000, FileOptions.SequentialScan));
        }
    }
}
