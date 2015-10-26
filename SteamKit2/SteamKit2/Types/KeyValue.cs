/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                // bool bAccepted = true;

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
                    // bAccepted = ( s == "[$WIN32]" );

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

    internal static class KeyValueUtils
    {
        // Method below copied from here: http://stackoverflow.com/questions/2811725/is-there-a-built-in-way-to-compare-ienumerablet-by-their-elements
        // Note, modified from original code.
        public static int SequenceCompare<T>( this IEnumerable<T> source1, IEnumerable<T> source2 )
        {
            if ( source1 == null )
                throw new ArgumentNullException( "source1" );
            else if ( source1 == null )
                throw new ArgumentNullException( "source2" );
            using ( IEnumerator<T> iterator1 = source1.GetEnumerator() )
            using ( IEnumerator<T> iterator2 = source2.GetEnumerator() )
            {
                while ( true )
                {
                    bool next1 = iterator1.MoveNext();
                    bool next2 = iterator2.MoveNext();

                    if ( !next1 && !next2 ) // Both sequences finished
                    {
                        return 0;
                    }

                    if ( !next1 ) // Only the first sequence has finished
                    {
                        return -1;
                    }

                    if ( !next2 ) // Only the second sequence has finished
                    {
                        return 1;
                    }

                    int comparison = -1;

                    if ( typeof( T ).IsAssignableFrom( typeof( IComparable<T> ) ) )
                        comparison = ( ( IComparable<T> )iterator1.Current ).CompareTo( iterator2.Current );
                    else if ( typeof( T ).IsAssignableFrom( typeof( IComparable ) ) )
                        comparison = ( ( IComparable )iterator1.Current ).CompareTo( iterator2.Current );
                    else
                        Comparer<T>.Default.Compare( iterator1.Current, iterator2.Current );

                    // If elements are non-equal, we're done
                    if ( comparison != 0 )
                    {
                        return comparison;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a recursive string key to arbitrary value container.
    /// </summary>
    public class KeyValue : IConvertible, IDictionary<string, KeyValue>, IEquatable<KeyValue>, IComparable<KeyValue>, IComparable
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
        /// This gets the all the childrens' <see cref="Name"/> of this <see cref="KeyValue"/>.
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                return Children.ConvertAll( c => c.Name );
            }
        }

        /// <summary>
        /// This gets all the children of this <see cref="KeyValue"/>.
        /// </summary>
        public ICollection<KeyValue> Values
        {
            get
            {
                return Children;
            }
        }

        /// <summary>
        /// This returns the number of children this <see cref="KeyValue"/> has.
        /// </summary>
        public int Count
        {
            get
            {
                return Children.Count;
            }
        }

        /// <summary>
        /// Is this readonly?
        /// </summary>
        /// <value><c>false</c>.</value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets or sets the child <see cref="SteamKit2.KeyValue" /> with the specified key.
        /// When retrieving by key, if no child with the given key exists, <see cref="Invalid" /> is returned.
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
            set
            {
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
        /// Returns the <see cref="TypeCode"/> for class <see cref="KeyValue"/>.
        /// </summary>
        /// <returns>The enumerated constant, <see cref="TypeCode.String"/>.</returns>
        public TypeCode GetTypeCode()
        {
            return this.Value.GetTypeCode();
        }

        bool IConvertible.ToBoolean( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToBoolean( provider );
        }

        char IConvertible.ToChar( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToChar( provider );
        }

        sbyte IConvertible.ToSByte( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToSByte( provider );
        }

        byte IConvertible.ToByte( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToByte( provider );
        }

        short IConvertible.ToInt16( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToInt16( provider );
        }

        ushort IConvertible.ToUInt16( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToUInt16( provider );
        }

        int IConvertible.ToInt32( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToInt32( provider );
        }

        uint IConvertible.ToUInt32( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToUInt32( provider );
        }

        long IConvertible.ToInt64( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToInt64( provider );
        }

        ulong IConvertible.ToUInt64( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToUInt64( provider );
        }

        float IConvertible.ToSingle( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToSingle( provider );
        }

        double IConvertible.ToDouble( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToDouble( provider );
        }

        decimal IConvertible.ToDecimal( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToDecimal( provider );
        }

        DateTime IConvertible.ToDateTime( IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToDateTime( provider );
        }

        /// <summary>
        /// Returns the value of <see cref="Value"/>.
        /// </summary>
        /// <param name="provider">(Reserved) An object that supplies culture-specific formatting information.</param>
        /// <returns>The current value of <see cref="Value"/>.</returns>
        public string ToString( IFormatProvider provider )
        {
            return this.Value;
        }

        object IConvertible.ToType( System.Type conversionType, IFormatProvider provider )
        {
            return ( ( IConvertible )this.Value ).ToType( conversionType, provider );
        }

        /// <summary>
        /// This checks for a child at key.
        /// </summary>
        /// <param name="key">Key to look for.</param>
        /// <returns><c>true</c> if child exists, <c>false</c> otherwise.</returns>
        public bool ContainsKey( string key )
        {
            return Children.Any( c => string.Equals( c.Name, key, StringComparison.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// This sets the child <see cref="SteamKit2.KeyValue" /> with the specified key.
        /// </summary>
        /// <param name="key">Key to set.</param>
        /// <param name="value">Value to set.</param>
        public void Add( string key, KeyValue value )
        {
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

        /// <summary>
        /// This attempts to remove the child at key.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns><c>true</c> if removed or doesn't exist, <c>false</c> otherwise.</returns>
        public bool Remove( string key )
        {
            var value = Children.FirstOrDefault( c => string.Equals( c.Name, key, StringComparison.OrdinalIgnoreCase ) );
            if ( value != null )
                return Children.Remove( value );
            return false;
        }

        /// <summary>
        /// This attempts to retrieve the value at key.
        /// </summary>
        /// <param name="key">Key to find.</param>
        /// <param name="value">Location to store the found value on success, <see cref="Invalid"/> if key not found.</param>
        /// <returns><c>true</c> if value found, <c>false</c> otherwise.</returns>
        public bool TryGetValue( string key, out KeyValue value )
        {
            value = Children.FirstOrDefault( c => string.Equals( c.Name, key, StringComparison.OrdinalIgnoreCase ) );
            if ( value == null )
                value = Invalid;
            return value != Invalid;
        }

        /// <summary>
        /// This sets the child <see cref="SteamKit2.KeyValue" /> with the specified <see cref="KeyValuePair{TKey, TValue}.Key"/>.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to add.</param>
        public void Add( KeyValuePair<string, KeyValue> item )
        {
            var existingChild = this.Children
                .FirstOrDefault( c => string.Equals( c.Name, item.Key, StringComparison.OrdinalIgnoreCase ) );

            if ( existingChild != null )
            {
                // if the key already exists, remove the old one
                this.Children.Remove( existingChild );
            }

            // ensure the given KV actually has the correct key assigned
            item.Value.Name = item.Key;

            this.Children.Add( item.Value );
        }

        /// <summary>
        /// This clears this <see cref="KeyValue"/> of all children.
        /// </summary>
        public void Clear()
        {
            Children.Clear();
        }

        /// <summary>
        /// This checks to see if the specified <see cref="KeyValuePair{TKey, TValue}"/> exists as a child.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to lookup.</param>
        /// <returns><c>true</c> if match found, <c>false</c> otherwise.</returns>
        public bool Contains( KeyValuePair<string, KeyValue> item )
        {
            return Children.Any( c => c.Equals( item ) );
        }

        /// <summary>
        /// Copies the elements of this <see cref="KeyValue"/>'s children to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of
        /// the elements copied from this <see cref="KeyValue"/>'s children.
        /// The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo( KeyValuePair<string, KeyValue>[] array, int arrayIndex )
        {
            Children.ConvertAll<KeyValuePair<string, KeyValue>>( c => c ).CopyTo( array, arrayIndex );
        }

        /// <summary>
        /// This attempts to remove the child at <see cref="KeyValuePair{TKey, TValue}.Key"/>.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to remove.</param>
        /// <returns><c>true</c> if removed or doesn't exist, <c>false</c> otherwise.</returns>
        public bool Remove( KeyValuePair<string, KeyValue> item )
        {
            var value = Children.FirstOrDefault( c => c.Equals( item ) );
            if ( value != null )
                return Children.Remove( value );
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, KeyValue>> GetEnumerator()
        {
            return Children.ConvertAll<KeyValuePair<string, KeyValue>>( c => c ).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Children.ConvertAll<KeyValuePair<string, KeyValue>>( c => c ).GetEnumerator();
        }

        /// <summary>
        /// Returns whether this <see cref="KeyValue"/> is equals to another <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="other">Other <see cref="KeyValue"/> to compare.</param>
        /// <returns><c>true</c> if equal, <c>false</c> otherwise.</returns>
        public bool Equals( KeyValue other )
        {
            return string.Equals( Name, other.Name, StringComparison.OrdinalIgnoreCase ) &&
                string.Equals( Value, other.Value, StringComparison.OrdinalIgnoreCase ) &&
                Children.OrderBy( c => c ).SequenceEqual( other.Children.OrderBy( c => c ) );
        }

        /// <summary>
        /// Compares this <see cref="KeyValue"/> to another <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="other">Other <see cref="KeyValue"/> to compare.</param>
        /// <returns><c>1</c> if this is greater, <c>0</c> if equal, <c>-1</c> is this is lesser.</returns>
        public int CompareTo( KeyValue other )
        {
            if ( other == null )
                return -1;
            else if ( !string.Equals( Name, other.Name, StringComparison.OrdinalIgnoreCase ) )
                return string.Compare( Name, other.Name, true );
            else if ( !string.Equals( Value, other.Value, StringComparison.OrdinalIgnoreCase ) )
                return string.Compare( Value, other.Value, true );
            else if ( !Children.OrderBy( c => c ).SequenceEqual( other.Children.OrderBy( c => c ) ) )
                return Children.OrderBy( c => c ).SequenceCompare( other.Children.OrderBy( c => c ) );
            return 0;
        }

        /// <summary>
        /// Compares this <see cref="KeyValue"/> to a <see cref="object"/>.
        /// </summary>
        /// <param name="other">The <see cref="object"/> to compare.</param>
        /// <returns><c>1</c> if this is greater, <c>0</c> if equal, <c>-1</c> is this is lesser.</returns>
        public int CompareTo( object other )
        {
            if ( other == null )
                return -1;
            KeyValue c = other as KeyValue;
            if ( c == null )
                return -1;
            return CompareTo( c );
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

            if ( Enum.TryParse( this.Value, out value ) == false )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
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
        [Obsolete( "Use TryReadAsBinary instead. Note that TryLoadAsBinary returns the root object, not a dummy parent node containg the root object." )]
        public static KeyValue LoadAsBinary( string path )
        {
            var kv = LoadFromFile( path, true );
            if ( kv == null )
            {
                return null;
            }

            var parent = new KeyValue();
            parent.Children.Add( kv );
            return parent;
        }

        /// <summary>
        /// Attempts to load the given filename as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <param name="keyValue">The resulting <see cref="KeyValue"/> object if the load was successful, or <c>null</c> if unsuccessful.</param>
        /// <returns><c>true</c> if the load was successful, or <c>false</c> on failure.</returns>
        public static bool TryLoadAsBinary( string path, out KeyValue keyValue )
        {
            keyValue = LoadFromFile( path, true );
            return keyValue != null;
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

            new KVTextReader( this, input );

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
                    // bAccepted = ( value == "[$WIN32]" );
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
            if ( asBinary )
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
                f.WriteNullTermString( Name, Encoding.UTF8 );
                foreach ( var child in Children )
                {
                    child.RecursiveSaveBinaryToStreamCore( f );
                }
                f.WriteByte( ( byte )Type.End );
            }
            else
            {
                f.WriteByte( ( byte )Type.String );
                f.WriteNullTermString( Name, Encoding.UTF8 );
                f.WriteNullTermString( Value ?? string.Empty, Encoding.UTF8 );
            }
        }

        private void RecursiveSaveTextToFile( Stream stream, int indentLevel = 0 )
        {
            // write header
            WriteIndents( stream, indentLevel );
            WriteString( stream, Name, true );
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
                    WriteString( stream, child.Name, true );
                    WriteString( stream, "\t\t" );
                    WriteString( stream, child.AsString(), true );
                    WriteString( stream, "\n" );
                }
            }

            WriteIndents( stream, indentLevel );
            WriteString( stream, "}\n" );
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
        [Obsolete( "Use TryReadAsBinary instead. Note that TryReadAsBinary returns the root object, not a dummy parent node containg the root object." )]
        public bool ReadAsBinary( Stream input )
        {
            var dummyChild = new KeyValue();
            this.Children.Add( dummyChild );
            return dummyChild.TryReadAsBinary( input );
        }

        /// <summary>
        /// Populate this instance from the given <see cref="Stream"/> as a binary <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="input">The input <see cref="Stream"/> to read from.</param>
        /// <returns><c>true</c> if the read was successful; otherwise, <c>false</c>.</returns>
        public bool TryReadAsBinary( Stream input )
        {
            return TryReadAsBinaryCore( input, this, null );
        }

        static bool TryReadAsBinaryCore( Stream input, KeyValue current, KeyValue parent )
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
                            DebugLog.WriteLine( "KeyValue", "Encountered WideString type when parsing binary KeyValue, which is unsupported. Returning false." );
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

                    default:
                        {
                            return false;
                        }
                }

                if ( parent != null )
                {
                    parent.Children.Add( current );
                }
                current = new KeyValue();
            }

            return true;
        }

        /// <summary>
        /// Converts a <see cref="KeyValue"/> into a <see cref="KeyValuePair{TKey, TValue}"/>.
        /// </summary>
        /// <param name="c">The <see cref="KeyValue"/> to convert.</param>
        public static implicit operator KeyValuePair<string, KeyValue>( KeyValue c )
        {
            if ( c == null )
                return default( KeyValuePair<string, KeyValue> );
            return new KeyValuePair<string, KeyValue>( c.Name, c );
        }

        /// <summary>
        /// Converts a <see cref="KeyValuePair{TKey, TValue}"/> into a <see cref="KeyValue"/>.
        /// </summary>
        /// <param name="c">The <see cref="KeyValuePair{TKey, TValue}"/> to convert.</param>
        public static implicit operator KeyValue( KeyValuePair<string, KeyValue> c )
        {
            c.Value.Name = c.Key;
            return c.Value;
        }
    }
}