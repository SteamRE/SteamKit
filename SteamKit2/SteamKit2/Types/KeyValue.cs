using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteamKit2
{
    public class KeyValue
    {
        public enum Type : byte
        {
            None = 0,
            String = 1,
            Int32 = 2,
            Float32 = 3,
            Pointer = 4,
            WideString = 5,
            Color = 6,
            UInt64 = 7,
            End = 8,
        }

        private static KeyValue Invalid = new KeyValue();
        public string Name = "<root>";
        public Type ValueType = Type.None;
        public object Value;
        public bool Valid;

        public List<KeyValue> Children = null;

        public KeyValue this[string key]
        {
            get
            {
                if (this.Children == null)
                {
                    return Invalid;
                }

                var child = this.Children.SingleOrDefault(
                    c => c.Name.ToLowerInvariant() == key.ToLowerInvariant());

                if (child == null)
                {
                    return Invalid;
                }

                return child;
            }
        }

        public string AsString(string defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }
            else if (this.Value == null)
            {
                return defaultValue;
            }

            return this.Value.ToString();
        }

        public int AsInteger(int defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }

            switch (this.ValueType)
            {
                case Type.String:
                case Type.WideString:
                    {
                        int value;
                        if (int.TryParse((string)this.Value, out value) == false)
                        {
                            return defaultValue;
                        }
                        return value;
                    }

                case Type.Int32:
                    {
                        return (int)this.Value;
                    }

                case Type.Float32:
                    {
                        return (int)((float)this.Value);
                    }

                case Type.UInt64:
                    {
                        return (int)((ulong)this.Value & 0xFFFFFFFF);
                    }
            }

            return defaultValue;
        }

        public float AsFloat(float defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }

            switch (this.ValueType)
            {
                case Type.String:
                case Type.WideString:
                    {
                        float value;
                        if (float.TryParse((string)this.Value, out value) == false)
                        {
                            return defaultValue;
                        }
                        return value;
                    }

                case Type.Int32:
                    {
                        return (float)((int)this.Value);
                    }

                case Type.Float32:
                    {
                        return (float)this.Value;
                    }

                case Type.UInt64:
                    {
                        return (float)((ulong)this.Value & 0xFFFFFFFF);
                    }
            }

            return defaultValue;
        }

        public bool AsBoolean(bool defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }

            switch (this.ValueType)
            {
                case Type.String:
                case Type.WideString:
                    {
                        int value;
                        if (int.TryParse((string)this.Value, out value) == false)
                        {
                            return defaultValue;
                        }
                        return value != 0 ? true : false;
                    }

                case Type.Int32:
                    {
                        return ((int)this.Value) != 0 ? true : false;
                    }

                case Type.Float32:
                    {
                        return ((int)((float)this.Value)) != 0.0f ? true : false;
                    }

                case Type.UInt64:
                    {
                        return ((ulong)this.Value) != 0 ? true : false;
                    }
            }

            return defaultValue;
        }

        public override string ToString()
        {
            if (this.Valid == false)
            {
                return "<invalid>";
            }

            if (this.ValueType == Type.None)
            {
                return this.Name;
            }

            return string.Format("{0} = {1}", this.Name, this.Value);
        }

        public static KeyValue LoadAsBinary(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            try
            {
                var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var kv = new KeyValue();

                if (kv.ReadAsBinary(input) == false)
                {
                    return null;
                }

                input.Close();
                return kv;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ReadAsBinary(Stream input)
        {
            this.Children = new List<KeyValue>();

            try
            {
                while (true)
                {
                    
                    var type = (Type)input.ReadByte();

                    if (type == Type.End)
                    {
                        break;
                    }

                    var current = new KeyValue();
                    current.ValueType = type;
                    current.Name = input.ReadNullTermString( Encoding.UTF8 );

                    switch (type)
                    {
                        case Type.None:
                            {
                                current.ReadAsBinary(input);
                                break;
                            }

                        case Type.String:
                            {
                                current.Valid = true;
                                current.Value = input.ReadNullTermString( Encoding.UTF8 );
                                break;
                            }

                        case Type.WideString:
                            {
                                throw new FormatException("wstring is unsupported");
                            }

                        case Type.Int32:
                            {
                                current.Valid = true;
                                current.Value = input.ReadInt32();
                                break;
                            }

                        case Type.UInt64:
                            {
                                current.Valid = true;
                                current.Value = input.ReadUInt64();
                                break;
                            }

                        case Type.Float32:
                            {
                                current.Valid = true;
                                current.Value = input.ReadFloat();
                                break;
                            }

                        case Type.Color:
                            {
                                current.Valid = true;
                                current.Value = (uint)input.ReadInt32();
                                break;
                            }

                        case Type.Pointer:
                            {
                                current.Valid = true;
                                current.Value = (uint)input.ReadInt32();
                                break;
                            }

                        default:
                            {
                                throw new FormatException();
                            }
                    }

                    if (input.Position >= input.Length)
                    {
                        throw new FormatException();
                    }

                    this.Children.Add(current);
                }

                this.Valid = true;
                return input.Position == input.Length;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}