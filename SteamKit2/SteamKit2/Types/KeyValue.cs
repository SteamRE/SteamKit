/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    class KVTextReader : StreamReader
    {
        internal static Dictionary<char, char> escapedMapping = new Dictionary<char, char>
        {
            { '\\', '\\' },
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

            KeyValue? currentKey = kv;

            do
            {
                // bool bAccepted = true;

                var s = ReadToken( out wasQuoted, out wasConditional );

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
                    // bAccepted = ( s == "[$WIN32]" );

                    // Now get the '{'
                    s = ReadToken( out wasQuoted, out wasConditional );
                }

                if ( s != null && s.StartsWith( "{" ) && !wasQuoted )
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
                    ReadLine();
                    return true;
                    /*
                     *  As came up in parsing the Dota 2 units.txt file, the reference (Valve) implementation
                     *  of the KV format considers a single forward slash to be sufficient to comment out the
                     *  entirety of a line. While they still _tend_ to use two, it's not required, and likely
                     *  is just done out of habit.
                     */
                }

                return false;
            }

            return false;
        }

        public string? ReadToken( out bool wasQuoted, out bool wasConditional )
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
            Int64 = 10,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValue"/> class.
        /// </summary>
        /// <param name="name">The optional name of the root key.</param>
        /// <param name="value">The optional value assigned to the root key.</param>
        public KeyValue( string? name = null, string? value = null )
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
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the value of this instance.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Gets the children of this instance.
        /// </summary>
        public List<KeyValue> Children { get; private set; }


        /// <summary>
        /// Gets or sets the child <see cref="SteamKit2.KeyValue" /> with the specified key.
        /// When retrieving by key, if no child with the given key exists, <see cref="Invalid" /> is returned.
        /// </summary>
        public KeyValue this[ string key ]
        {
            get
            {
                if ( key == null )
                {
                    throw new ArgumentNullException( nameof(key) );
                }

                var child = this.Children
                    .FirstOrDefault( c => string.Equals( c.Name, key, StringComparison.OrdinalIgnoreCase ) );

                if ( child == null )
                {
                    return Invalid;
                }

                return child;
            }
            set
            {
                if ( key == null )
                {
                    throw new ArgumentNullException( nameof(key) );
                }

                var existingChild = this.Children
                    .FirstOrDefault( c => string.Equals( c.Name, key, StringComparison.OrdinalIgnoreCase ) );

                if ( existingChild != null )
                {
                    // if the key already exists, remove the old one
                    this.Children.Remove( existingChild );
                }

                // ensure the given KV actually has the correct key assigned
                value.Name = key;

                this.Children.Add( value );
            }
        }

        /// <summary>
        /// Returns the value of this instance as a string.
        /// </summary>
        /// <returns>The value of this instance as a string.</returns>
        public string? AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an unsigned byte.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned byte.</returns>
        public byte AsUnsignedByte( byte defaultValue = default( byte ) )
        {
            byte value;

            if ( byte.TryParse( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Attempts to convert and return the value of this instance as an unsigned short.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned short.</returns>
        public ushort AsUnsignedShort( ushort defaultValue = default( ushort ) )
        {
            ushort value;

            if ( ushort.TryParse( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value;
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
        /// Attempts to convert and return the value of this instance as an unsigned integer.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned integer.</returns>
        public uint AsUnsignedInteger( uint defaultValue = default( uint ) )
        {
            uint value;

            if ( uint.TryParse( this.Value, out value ) == false )
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
        /// <returns>The value of this instance as a long.</returns>
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
        /// Attempts to convert and return the value of this instance as an unsigned long.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an unsigned long.</returns>
        public ulong AsUnsignedLong( ulong defaultValue = default( ulong ) )
        {
            ulong value;

            if ( ulong.TryParse( this.Value, out value ) == false )
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
        /// <returns>The value of this instance as a float.</returns>
        public float AsFloat( float defaultValue = default( float ) )
        {
            float value;

            if ( float.TryParse( this.Value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value ) == false )
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
        /// <returns>The value of this instance as a boolean.</returns>
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
        /// Attempts to convert and return the value of this instance as an enum.
        /// If the conversion is invalid, the default value is returned.
        /// </summary>
        /// <param name="defaultValue">The default value to return if the conversion is invalid.</param>
        /// <returns>The value of this instance as an enum.</returns>
        public T AsEnum<T>( T defaultValue = default( T ) )
            where T : struct
        {
            T value;

            if ( Enum.TryParse<T>( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value;
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
        public static KeyValue? LoadAsText( string path )
        {
            return LoadFromFile( path, false );
        }

        /// <summary>
        /// Attempts to load the given filename as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <param name="keyValue">The resulting <see cref="KeyValue"/> object if the load was successful, or <c>null</c> if unsuccessful.</param>
        /// <returns><c>true</c> if the load was successful, or <c>false</c> on failure.</returns>
        public static bool TryLoadAsBinary( string path, [NotNullWhen(true)] out KeyValue? keyValue )
        {
            keyValue = LoadFromFile(path, true);
            return keyValue != null;
        }


        static KeyValue? LoadFromFile( string path, bool asBinary )
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
                        if ( kv.TryReadAsBinary( input ) == false )
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
        public static KeyValue? LoadFromString( string input )
        {
            if ( input == null )
            {
                throw new ArgumentNullException( nameof(input) );
            }

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
            if ( input == null )
            {
                throw new ArgumentNullException( nameof(input) );
            }

            this.Children = new List<KeyValue>();

            using var _ = new KVTextReader( this, input );

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
                // bool bAccepted = true;

                // get the key name
                var name = kvr.ReadToken( out wasQuoted, out wasConditional );

                if ( name is null || name.Length == 0 )
                {
                    throw new InvalidDataException( "RecursiveLoadFromBuffer: got EOF or empty keyname" );
                }

                if ( name.StartsWith( "}" ) && !wasQuoted )	// top level closed, stop reading
                {
                    break;
                }

                KeyValue dat = new KeyValue( name );
                dat.Children = new List<KeyValue>();
                this.Children.Add( dat );

                // get the value
                string? value = kvr.ReadToken( out wasQuoted, out wasConditional );

                if ( wasConditional && value != null )
                {
                    // bAccepted = ( value == "[$WIN32]" );
                    value = kvr.ReadToken( out wasQuoted, out wasConditional );
                }

                if ( value == null )
                {
                    throw new Exception( "RecursiveLoadFromBuffer:  got NULL key" );
                }

                if ( value.StartsWith( "}" ) && !wasQuoted )
                {
                    throw new Exception( "RecursiveLoadFromBuffer:  got } in key" );
                }

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
            using ( var f = File.Create( path ) )
            {
                SaveToStream( f, asBinary );
            }
        }

        /// <summary>
        /// Saves this instance to a given <see cref="System.IO.Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="System.IO.Stream"/> to save to.</param>
        /// <param name="asBinary">If set to <c>true</c>, saves this instance as binary.</param>
        public void SaveToStream( Stream stream, bool asBinary )
        {
            if ( stream == null )
            {
                throw new ArgumentNullException( nameof(stream) );
            }

            if (asBinary)
            {
                RecursiveSaveBinaryToStream( stream );
            }
            else
            {
                RecursiveSaveTextToFile( stream );
            }
        }

        void RecursiveSaveBinaryToStream( Stream f )
        {
            RecursiveSaveBinaryToStreamCore( f );
            f.WriteByte( ( byte )Type.End );
        }

        void RecursiveSaveBinaryToStreamCore( Stream f )
        {
            // Only supported types ATM:
            // 1. KeyValue with children (no value itself)
            // 2. String KeyValue
            if ( Children.Any() )
            {
                f.WriteByte( ( byte )Type.None );
                f.WriteNullTermString( GetNameForSerialization(), Encoding.UTF8 );
                foreach ( var child in Children )
                {
                    child.RecursiveSaveBinaryToStreamCore( f );
                }
                f.WriteByte( ( byte )Type.End );
            }
            else
            {
                f.WriteByte( ( byte )Type.String );
                f.WriteNullTermString( GetNameForSerialization(), Encoding.UTF8 );
                f.WriteNullTermString( Value ?? string.Empty, Encoding.UTF8 );
            }
        }

        private void RecursiveSaveTextToFile( Stream stream, int indentLevel = 0 )
        {
            // write header
            WriteIndents( stream, indentLevel );
            WriteString( stream, GetNameForSerialization(), true );
            WriteString( stream, "\n" );
            WriteIndents( stream, indentLevel );
            WriteString( stream, "{\n" );

            // loop through all our keys writing them to disk
            foreach ( KeyValue child in Children )
            {
                if ( child.Value == null )
                {
                    child.RecursiveSaveTextToFile( stream, indentLevel + 1 );
                }
                else
                {
                    WriteIndents( stream, indentLevel + 1 );
                    WriteString( stream, child.GetNameForSerialization(), true );
                    WriteString( stream, "\t\t" );
                    WriteString( stream, EscapeText( child.Value ), true );
                    WriteString( stream, "\n" );
                }
            }

            WriteIndents( stream, indentLevel );
            WriteString( stream, "}\n" );
        }

        static string EscapeText( string value )
        {
            foreach ( var kvp in KVTextReader.escapedMapping )
            {
                var textToReplace = new string( kvp.Value, 1 );
                var escapedReplacement = @"\" + kvp.Key;
                value = value.Replace( textToReplace, escapedReplacement );
            }

            return value;
        }

        void WriteIndents( Stream stream, int indentLevel )
        {
            WriteString( stream, new string( '\t', indentLevel ) );
        }

        static void WriteString( Stream stream, string str, bool quote = false )
        {
            byte[] bytes = Encoding.UTF8.GetBytes( ( quote ? "\"" : "" ) + str.Replace( "\"", "\\\"" ) + ( quote ? "\"" : "" ) );
            stream.Write( bytes, 0, bytes.Length );
        }

        /// <summary>
        /// Populate this instance from the given <see cref="Stream"/> as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool TryReadAsBinary( Stream input )
        {
            if ( input == null )
            {
                throw new ArgumentNullException( nameof(input) );
            }
            
            return TryReadAsBinaryCore( input, this, null );
        }

        static bool TryReadAsBinaryCore( Stream input, KeyValue current, KeyValue? parent )
        {
            current.Children = new List<KeyValue>();

            while ( true )
            {
                var type = ( Type )input.ReadByte();

                if ( type == Type.End )
                {
                    break;
                }

                current.Name = input.ReadNullTermString( Encoding.UTF8 );
                
                switch ( type )
                {
                    case Type.None:
                        {
                            var child = new KeyValue();
                            var didReadChild = TryReadAsBinaryCore( input, child, current );
                            if ( !didReadChild )
                            {
                                return false;
                            }
                            break;
                        }

                    case Type.String:
                        {
                            current.Value = input.ReadNullTermString( Encoding.UTF8 );
                            break;
                        }

                    case Type.WideString:
                        {
                            DebugLog.WriteLine( "KeyValue", "Encountered WideString type when parsing binary KeyValue, which is unsupported. Returning false.");
                            return false;
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

                    case Type.Int64:
                        {
                            current.Value = Convert.ToString( input.ReadInt64() );
                            break;
                        }

                    default:
                        {
                            return false;
                        }
                }

                if (parent != null)
                {
                    parent.Children.Add(current);
                }
                current = new KeyValue();
            }

            return true;
        }

        string GetNameForSerialization()
        {
            if ( Name is null )
            {
                throw new InvalidOperationException( "Cannot serialise a KeyValue object with a null name!" );
            }

            return Name;
        }
    }
}
