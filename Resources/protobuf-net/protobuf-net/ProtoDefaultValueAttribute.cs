using System;
using System.ComponentModel;

namespace ProtoBuf
{
    /// <summary>
    /// Extension of DefaultValueAttribute to expose non-CLS compliant constructors
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    public sealed class ProtoDefaultValueAttribute : DefaultValueAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( bool value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( byte value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( char value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( double value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( float value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( int value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( long value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">An <see cref="T:System.Object"/> that represents the default value.</param>
        public ProtoDefaultValueAttribute( object value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( short value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( string value ) : base( value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( Type type, string value ) : base( type, value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( ushort value ) : base( ( object )value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( uint value ) : base( ( object )value ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ProtoDefaultValueAttribute( ulong value ) : base( ( object )value ) { }
    }
}