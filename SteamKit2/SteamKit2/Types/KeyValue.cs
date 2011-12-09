/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace SteamKit2
{
    public class KVTextReader : StreamReader
    {
        public KVTextReader( KeyValue kv, Stream input )
            : base( input )
        {
            bool wasQuoted;
            bool wasConditional;

            KeyValue currentKey = kv;

            do
            {
                bool bAccepted = true;

                string s = ReadToken( out wasQuoted, out wasConditional );
                if ( s == null || s == "" )
                    break;

                if ( currentKey == null )
                {
                    currentKey = new KeyValue( s );
                }
                else
                {
                    currentKey.Name = s;
                }

                s = ReadToken( out wasQuoted, out wasConditional );

                if ( wasConditional )
                {
                    bAccepted = ( s == "[$WIN32]" );

                    // Now get the '{'
                    s = ReadToken( out wasQuoted, out wasConditional );
                }

                if ( s.StartsWith( "{" ) && !wasQuoted )
                {
                    // header is valid so load the file
                    currentKey.RecursiveLoadFromBuffer( this );
                }
                else
                {
                    throw new Exception( "LoadFromBuffer: missing {" );
                }

                currentKey = null;
            }
            while ( !EndOfStream );
        }

        private void EatWhiteSpace()
        {
            while ( !EndOfStream )
            {
                if ( !Char.IsWhiteSpace( ( char )Peek() ) )
                {
                    break;
                }

                Read();
            }
        }

        private bool EatCPPComment()
        {
            if ( !EndOfStream )
            {
                char next = ( char )Peek();
                if ( next == '/' )
                {
                    Read();
                    if ( next == '/' )
                    {
                        ReadLine();
                        return true;
                    }
                    else
                    {
                        throw new Exception( "BARE / WHAT ARE YOU DOIOIOIINODGNOIGNONGOIGNGGGGGGG" );
                    }
                }

                return false;
            }

            return false;
        }

        public string ReadToken( out bool wasQuoted, out bool wasConditional )
        {
            wasQuoted = false;
            wasConditional = false;

            while ( true )
            {
                EatWhiteSpace();

                if ( EndOfStream )
                {
                    return null;
                }

                if ( !EatCPPComment() )
                {
                    break;
                }
            }

            if ( EndOfStream )
                return null;

            char next = ( char )Peek();
            if ( next == '"' )
            {
                wasQuoted = true;

                // "
                Read();

                var sb = new StringBuilder();
                while ( !EndOfStream && ( char )Peek() != '"' )
                {
                    sb.Append( ( char )Read() );
                }

                // "
                Read();

                return sb.ToString();
            }

            if ( next == '{' || next == '}' )
            {
                Read();
                return next.ToString();
            }

            bool bConditionalStart = false;
            int count = 0;
            var ret = new StringBuilder();
            while ( !EndOfStream )
            {
                next = ( char )Peek();

                if ( next == '"' || next == '{' || next == '}' )
                    break;

                if ( next == '[' )
                    bConditionalStart = true;

                if ( next == ']' && bConditionalStart )
                    wasConditional = true;

                if ( Char.IsWhiteSpace( next ) )
                    break;

                if ( count < 1023 )
                {
                    ret.Append( next );
                }
                else
                {
                    throw new Exception( "ReadToken overflow" );
                }

                Read();
            }

            return ret.ToString();
        }
    }

    public class KeyValue
    {
        private enum Type : byte
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

        public KeyValue()
        {
            Value = null;
            Children = new List<KeyValue>();
        }

        public KeyValue( string name ) : this()
        {
            Name = name;
        }

        private static KeyValue Invalid = new KeyValue();
        public string Name = "<root>";
        public string Value;

        public List<KeyValue> Children = null;

        public KeyValue this[ string key ]
        {
            get
            {
                var child = this.Children.SingleOrDefault(
                    c => c.Name.ToLowerInvariant() == key.ToLowerInvariant() );

                if ( child == null )
                {
                    return Invalid;
                }

                return child;
            }
        }

        public string AsString()
        {
            return this.Value;
        }

        public int AsInteger( int defaultValue )
        {
            int value;

            if (int.TryParse((string)this.Value, out value) == false)
            {
                return defaultValue;
            }

            return value;
        }

        public long AsLong( long defaultValue )
        {
            long value;

            if (long.TryParse((string)this.Value, out value) == false)
            {
                return defaultValue;
            }

            return value;
        }

        public float AsFloat( float defaultValue )
        {
            float value;
            
            if (float.TryParse((string)this.Value, out value) == false)
            {
                return defaultValue;
            }

            return value;
        }

        public bool AsBoolean( bool defaultValue )
        {
            int value;
            
            if (int.TryParse((string)this.Value, out value) == false)
            {
                return defaultValue;
            }

            return value != 0 ? true : false;
        }

        public override string ToString()
        {
            return string.Format( "{0} = {1}", this.Name, this.Value );
        }

        public static KeyValue LoadAsText( string path )
        {
            return LoadFromFile( path, false );
        }

        public static KeyValue LoadAsBinary( string path )
        {
            return LoadFromFile( path, true );
        }

        public static KeyValue LoadFromFile( string path, bool asBinary )
        {
            if ( File.Exists( path ) == false )
            {
                return null;
            }

            try
            {
                var input = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
                var kv = new KeyValue();

                if ( asBinary )
                {
                    if ( kv.ReadAsBinary( input ) == false )
                    {
                        return null;
                    }
                }
                else
                {
                    if ( kv.ReadAsText( input ) == false )
                    {
                        return null;
                    }
                }

                input.Close();
                return kv;
            }
            catch ( Exception )
            {
                return null;
            }
        }

        public static KeyValue LoadFromString( string input )
        {
            byte[] bytes = Encoding.UTF8.GetBytes( input );
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var kv = new KeyValue();

                try
                {
                    if (kv.ReadAsText(stream) == false)
                        return null;

                    return kv;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public bool ReadAsText( Stream input )
        {
            this.Children = new List<KeyValue>();

            KVTextReader kvr = new KVTextReader( this, input );

            return true;
        }

        public void RecursiveLoadFromBuffer( KVTextReader kvr )
        {
            bool wasQuoted;
            bool wasConditional;

            while ( true )
            {
                bool bAccepted = true;

                // get the key name
                string name = kvr.ReadToken( out wasQuoted, out wasConditional );

                if ( name == null )
                {
                    throw new Exception( "RecursiveLoadFromBuffer:  got EOF instead of keyname" );
                }

                if ( name == "" )
                {
                    throw new Exception( "RecursiveLoadFromBuffer:  got empty keyname" );
                }

                if ( name.StartsWith( "}" ) && !wasQuoted )	// top level closed, stop reading
                    break;

                KeyValue dat = new KeyValue( name );
                dat.Children = new List<KeyValue>();
                this.Children.Add( dat );

                // get the value
                string value = kvr.ReadToken( out wasQuoted, out wasConditional );

                if ( wasConditional && value != null )
                {
                    bAccepted = ( value == "[$WIN32]" );
                    value = kvr.ReadToken( out wasQuoted, out wasConditional );
                }

                if ( value == null )
                    throw new Exception( "RecursiveLoadFromBuffer:  got NULL key" );

                if ( value.StartsWith( "}" ) && !wasQuoted )
                    throw new Exception( "RecursiveLoadFromBuffer:  got } in key" );

                if ( value.StartsWith( "{" ) && !wasQuoted )
                {
                    dat.RecursiveLoadFromBuffer( kvr );
                }
                else
                {
                    if ( wasConditional )
                    {
                        throw new Exception( "RecursiveLoadFromBuffer:  got conditional between key and value" );
                    }

                    dat.Value = value;
                    // blahconditionalsdontcare
                }
            }
        }

        public void SaveToFile( string path, bool asBinary )
        {
            if ( asBinary )
                throw new NotImplementedException();

            var f = File.Create( path );

            RecursiveSaveToFile( f );

            f.Close();
        }

        private void RecursiveSaveToFile( FileStream f )
        {
            RecursiveSaveToFile( f, 0 );
        }

        private void RecursiveSaveToFile( FileStream f, int indentLevel )
        {
            // write header
            WriteIndents( f, indentLevel );
            WriteString( f, Name, true );
            WriteString( f, "\n" );
            WriteIndents( f, indentLevel );
            WriteString( f, "{\n" );

            // loop through all our keys writing them to disk
            foreach ( KeyValue child in Children )
            {
                if ( child.Value == null )
                {
                    child.RecursiveSaveToFile( f, indentLevel + 1 );
                }
                else
                {
                    WriteIndents( f, indentLevel + 1 );
                    WriteString( f, child.Name, true );
                    WriteString( f, "\t\t" );
                    WriteString( f, child.AsString(), true );
                    WriteString( f, "\n" );
                }
            }

            WriteIndents( f, indentLevel );
            WriteString( f, "}\n" );
        }

        private void WriteIndents( FileStream f, int indentLevel )
        {
            for ( int i = 0 ; i < indentLevel ; i++ )
            {
                f.WriteByte( ( byte )'\t' );
            }
        }

        private void WriteString( FileStream f, string str, bool quote = false )
        {
            byte[] bytes = Encoding.ASCII.GetBytes( ( quote ? "\"" : "" ) + str.Replace( "\"", "\\\"" ) + ( quote ? "\"" : "" ) );
            f.Write( bytes, 0, bytes.Length );
        }

        public bool ReadAsBinary( Stream input )
        {
            this.Children = new List<KeyValue>();

            try
            {
                while ( true )
                {

                    var type = ( Type )input.ReadByte();

                    if ( type == Type.End )
                    {
                        break;
                    }

                    var current = new KeyValue();
                    current.Name = input.ReadNullTermString( Encoding.UTF8 );

                    switch ( type )
                    {
                        case Type.None:
                            {
                                current.ReadAsBinary( input );
                                break;
                            }

                        case Type.String:
                            {
                                current.Value = input.ReadNullTermString( Encoding.UTF8 );
                                break;
                            }

                        case Type.WideString:
                            {
                                throw new FormatException( "wstring is unsupported" );
                            }

                        case Type.Int32:
                        case Type.Color:
                        case Type.Pointer:
                            {
                                current.Value = Convert.ToString(input.ReadInt32());
                                break;
                            }

                        case Type.UInt64:
                            {
                                current.Value = Convert.ToString(input.ReadUInt64());
                                break;
                            }

                        case Type.Float32:
                            {
                                current.Value = Convert.ToString(input.ReadFloat());
                                break;
                            }

                        default:
                            {
                                throw new FormatException();
                            }
                    }

                    if ( input.Position >= input.Length )
                    {
                        throw new FormatException();
                    }

                    this.Children.Add( current );
                }

                return input.Position == input.Length;
            }
            catch ( Exception )
            {
                return false;
            }
        }
    }
}