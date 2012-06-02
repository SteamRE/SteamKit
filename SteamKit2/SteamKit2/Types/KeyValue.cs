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
    class KVTextReader : StreamReader
    {
        static Dictionary<char, char> escapedMapping = new Dictionary<char, char>
        {
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' },
            // todo: any others?
        };

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

                if ( string.IsNullOrEmpty( s ) )
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
                while ( !EndOfStream )
                {
                    if ( Peek() == '\\' )
                    {
                        Read();

                        char escapedChar = ( char )Read();
                        char replacedChar;

                        if ( escapedMapping.TryGetValue( escapedChar, out replacedChar ) )
                            sb.Append( replacedChar );
                        else
                            sb.Append( escapedChar );

                        continue;
                    }

                    if ( Peek() == '"' )
                        break;

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

    /// <summary>
    /// Represents a recursive string key to arbitrary value container.
    /// </summary>
    public class KeyValue
    {
        enum Type : byte
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

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValue"/> class.
        /// </summary>
        /// <param name="name">The optional name of the root key.</param>
        /// <param name="value">The optional value assigned to the root key.</param>
        public KeyValue( string name = null, string value = null )
        {
            this.Name = name;
            this.Value = value;

            Children = new List<KeyValue>();
        }

        /// <summary>
        /// Represents an invalid <see cref="KeyValue"/> given when a searched for child does not exist.
        /// </summary>
        public readonly static KeyValue Invalid = new KeyValue();

        /// <summary>
        /// Gets or sets the name of this instance.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the value of this instance.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the children of this instance.
        /// </summary>
        public List<KeyValue> Children { get; private set; }


        /// <summary>
        /// Gets the child <see cref="SteamKit2.KeyValue"/> with the specified key.
        /// If no child with this key exists, <see cref="Invalid"/> is returned.
        /// </summary>
        public KeyValue this[ string key ]
        {
            get
            {
                var child = this.Children
                    .FirstOrDefault( c => string.Equals( c.Name, key, StringComparison.OrdinalIgnoreCase ) );

                if ( child == null )
                {
                    return Invalid;
                }

                return child;
            }
        }

        /// <summary>
        /// Returns the value of this instance as a string.
        /// </summary>
        /// <returns>The value of this instance as a string.</returns>
        public string AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an integer.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an integer.</returns>
        public int AsInteger( int defaultValue = default( int ) )
        {
            int value;

            if ( int.TryParse( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as a long.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an long.</returns>
        public long AsLong( long defaultValue = default( long ) )
        {
            long value;

            if ( long.TryParse( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as a float.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an float.</returns>
        public float AsFloat( float defaultValue = default( float ) )
        {
            float value;

            if ( float.TryParse( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as a boolean.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an boolean.</returns>
        public bool AsBoolean( bool defaultValue = default( bool ) )
        {
            int value;

            if ( int.TryParse( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value != 0;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} = {1}", this.Name, this.Value );
        }

        /// <summary>
        /// Attempts to load the given filename as a text <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>a <see cref="KeyValue"/> instance if the load was successful, or <c>null</c> on failure.</returns>
        /// <remarks>
        /// This method will swallow any exceptions that occur when reading, use <see cref="ReadAsText"/> if you wish to handle exceptions.
        /// </remarks>
        public static KeyValue LoadAsText( string path )
        {
            return LoadFromFile( path, false );
        }

        /// <summary>
        /// Attempts to load the given filename as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>a <see cref="KeyValue"/> instance if the load was successful, or <c>null</c> on failure.</returns>
        /// <remarks>
        /// This method will swallow any exceptions that occur when reading, use <see cref="ReadAsBinary"/> if you wish to handle exceptions.
        /// </remarks>
        public static KeyValue LoadAsBinary( string path )
        {
            return LoadFromFile( path, true );
        }


        static KeyValue LoadFromFile( string path, bool asBinary )
        {
            if ( File.Exists( path ) == false )
            {
                return null;
            }

            try
            {
                using ( var input = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
                {
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

                    return kv;
                }
            }
            catch ( Exception )
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to create an instance of <see cref="KeyValue"/> from the given input text.
        /// </summary>
        /// <param name="input">The input text to load.</param>
        /// <returns>a <see cref="KeyValue"/> instance if the load was successful, or <c>null</c> on failure.</returns>
        /// <remarks>
        /// This method will swallow any exceptions that occur when reading, use <see cref="ReadAsText"/> if you wish to handle exceptions.
        /// </remarks>
        public static KeyValue LoadFromString( string input )
        {
            byte[] bytes = Encoding.UTF8.GetBytes( input );

            using ( MemoryStream stream = new MemoryStream( bytes ) )
            {
                var kv = new KeyValue();

                try
                {
                    if ( kv.ReadAsText( stream ) == false )
                        return null;

                    return kv;
                }
                catch ( Exception )
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Populate this instance from the given <see cref="Stream"/> as a text <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool ReadAsText( Stream input )
        {
            this.Children = new List<KeyValue>();

            KVTextReader kvr = new KVTextReader( this, input );

            return true;
        }

        /// <summary>
        /// Opens and reads the given filename as text.
        /// </summary>
        /// <seealso cref="ReadAsText"/>
        /// <param name="filename">The file to open and read.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool ReadFileAsText( string filename )
        {
            using ( FileStream fs = new FileStream( filename, FileMode.Open ) )
            {
                return ReadAsText( fs );
            }
        }

        internal void RecursiveLoadFromBuffer( KVTextReader kvr )
        {
            bool wasQuoted;
            bool wasConditional;

            while ( true )
            {
                bool bAccepted = true;

                // get the key name
                string name = kvr.ReadToken( out wasQuoted, out wasConditional );

                if ( string.IsNullOrEmpty( name ) )
                {
                    throw new Exception( "RecursiveLoadFromBuffer: got EOF or empty keyname" );
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

        /// <summary>
        /// Saves this instance to file.
        /// </summary>
        /// <param name="path">The file path to save to.</param>
        /// <param name="asBinary">If set to <c>true</c>, saves this instance as binary.</param>
        public void SaveToFile( string path, bool asBinary )
        {
            if ( asBinary )
                throw new NotImplementedException();

            using ( var f = File.Create( path ) )
            {
                RecursiveSaveToFile( f );
            }
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
            WriteString( f, new string( '\t', indentLevel ) );
        }

        private static void WriteString( FileStream f, string str, bool quote = false )
        {
            byte[] bytes = Encoding.ASCII.GetBytes( ( quote ? "\"" : "" ) + str.Replace( "\"", "\\\"" ) + ( quote ? "\"" : "" ) );
            f.Write( bytes, 0, bytes.Length );
        }

        /// <summary>
        /// Populate this instance from the given <see cref="Stream"/> as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool ReadAsBinary( Stream input )
        {
            this.Children = new List<KeyValue>();

            while ( true )
            {

                var type = ( Type )input.ReadByte();

                if ( type == Type.End )
                {
                    break;
                }

                var current = new KeyValue();
                current.Name = input.ReadNullTermString( Encoding.UTF8 );

                try
                {
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
                                throw new InvalidDataException( "wstring is unsupported" );
                            }

                        case Type.Int32:
                        case Type.Color:
                        case Type.Pointer:
                            {
                                current.Value = Convert.ToString( input.ReadInt32() );
                                break;
                            }

                        case Type.UInt64:
                            {
                                current.Value = Convert.ToString( input.ReadUInt64() );
                                break;
                            }

                        case Type.Float32:
                            {
                                current.Value = Convert.ToString( input.ReadFloat() );
                                break;
                            }

                        default:
                            {
                                throw new InvalidDataException( "Unknown KV type encountered." );
                            }
                    }
                }
                catch ( InvalidDataException ex )
                {
                    throw new InvalidDataException( string.Format( "An exception ocurred while reading KV '{0}'", current.Name ), ex );
                }

                this.Children.Add( current );
            }

            return input.Position == input.Length;
        }
    }
}