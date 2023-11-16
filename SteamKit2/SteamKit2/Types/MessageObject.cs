using System;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// Represents a <see cref="KeyValue"/> backed MessageObject structure, which are often sent by the Steam servers.
    /// </summary>
    public class MessageObject
    {
        /// <summary>
        /// Gets the inner <see cref="KeyValue"/> object.
        /// </summary>
        public KeyValue KeyValues { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MessageObject"/> class, using the provided KeyValues object.
        /// </summary>
        /// <param name="keyValues">The KeyValue backing store for this message object.</param>
        public MessageObject( KeyValue keyValues )
        {
            ArgumentNullException.ThrowIfNull( keyValues );

            this.KeyValues = keyValues;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageObject"/> class with an empty inner KeyValues.
        /// </summary>
        public MessageObject()
        {
            this.KeyValues = new KeyValue( "MessageObject" );
        }


        /// <summary>
        /// Populates this MessageObject instance from the data inside the given stream.
        /// </summary>
        /// <param name="stream">The stream to load data from.</param>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        public bool ReadFromStream( Stream stream )
        {
            ArgumentNullException.ThrowIfNull( stream );

            return KeyValues.TryReadAsBinary( stream );
        }

        /// <summary>
        /// Writes this MessageObject instance to the given stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void WriteToStream( Stream stream )
        {
            ArgumentNullException.ThrowIfNull( stream );

            KeyValues.SaveToStream( stream, true );
        }
    }
}
