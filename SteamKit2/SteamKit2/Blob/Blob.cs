/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SteamKit2
{
    /// <summary>
    /// Represents an immutable blob object.
    /// </summary>
    public class Blob
    {
        /// <summary>
        /// Gets or the fields the blob contains.
        /// </summary>
        /// <value>The fields.</value>
        public List<BlobField> Fields { get; private set; }

        /// <summary>
        /// Gets the <see cref="SteamKit2.BlobField"/> with the specified descriptor.
        /// </summary>
        /// <value>The integer descriptor to lookup.</value>
        public BlobField this[ int descriptor ]
        {
            get { return LookupField( descriptor ); }
        }
        /// <summary>
        /// Gets the <see cref="SteamKit2.BlobField"/> with the specified descriptor.
        /// </summary>
        /// <value>The string descriptor to lookup.</value>
        public BlobField this[ string descriptor ]
        {
            get { return LookupField( descriptor ); }
        }

        /// <summary>
        /// Gets the cache state of the blob.
        /// </summary>
        /// <value>The cache state.</value>
        public ECacheState CacheState { get; private set; }
        /// <summary>
        /// Gets the process code of the blob.
        /// </summary>
        /// <value>The process code.</value>
        public EAutoPreprocessCode ProcessCode { get; private set; }

        /// <summary>
        /// Gets the compression level of the blob.
        /// </summary>
        /// <value>The compression level.</value>
        public int CompressionLevel { get; internal set; }
        /// <summary>
        /// Gets the IV used when encrypting or decrypting the blob.
        /// </summary>
        /// <value>The IV.</value>
        public byte[] IV { get; internal set; }

        /// <summary>
        /// Gets the spare data from parsing the blob.
        /// </summary>
        /// <value>The spare data.</value>
        public byte[] Spare { get; internal set; }


        // disallowing manual blob creation until the blob code is roundtrip
        internal Blob( ECacheState state, EAutoPreprocessCode code )
        {
            Fields = new List<BlobField>();

            CacheState = state;
            ProcessCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        /// <param name="data">The blob data.</param>
        public Blob( byte[] data )
        {
            Blob blob = BlobParser.ParseBlob( data );

            this.ProcessCode = blob.ProcessCode;
            this.CacheState = blob.CacheState;

            this.Fields = blob.Fields;

            this.CompressionLevel = blob.CompressionLevel;
            this.IV = blob.IV;

            this.Spare = blob.Spare;
        }


        /// <summary>
        /// Gets the raw data for a field with a specific string descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>Data if the specific field contains any data; otherwise null.</returns>
        public byte[] GetDescriptor( string descriptor )
        {
            BlobField field = LookupField( descriptor );

            if ( field == null || field.HasChildBlob() )
                return null;

            return field.Descriptor;
        }
        /// <summary>
        /// Gets the raw data for a field with a specific integer descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>Data if the specific field contains any data; otherwise null.</returns>
        public byte[] GetDescriptor( int descriptor )
        {
            BlobField field = LookupField( descriptor );

            if ( field == null || field.HasChildBlob() )
                return null;

            return field.Data;
        }


        /// <summary>
        /// Gets the string data for a field with a specific string descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>String data if the specific field contains string data; otherwise null.</returns>
        public string GetStringDescriptor( string descriptor )
        {
            BlobField field = LookupField( descriptor );

            if ( field == null || field.HasChildBlob() )
                return null;

            return field.GetStringData();
        }

        /// <summary>
        /// Gets the string data for a field with a specific integer descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>String data if the specific field contains string data; otherwise null.</returns>
        public string GetStringDescriptor( int descriptor )
        {
            BlobField field = LookupField( descriptor );

            if ( field == null || field.HasChildBlob() || !field.IsStringData() )
                return null;

            return field.GetStringData();
        }


        /// <summary>
        /// Gets the child blob for a field with a specific string descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>A blob oject if the specific field contains a childblob; otherwise, null.</returns>
        public Blob GetBlobDescriptor( string descriptor )
        {
            BlobField field = LookupField( descriptor );

            if ( field == null || !field.HasChildBlob() )
                return null;

            return field.GetChildBlob();
        }

        /// <summary>
        /// Gets the child blob for a field with a specific integer descriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns>A blob oject if the specific field contains a childblob; otherwise, null.</returns>
        public Blob GetBlobDescriptor( int descriptor )
        {
            BlobField field = LookupField( descriptor );

            if ( field == null || !field.HasChildBlob() )
                return null;

            return field.GetChildBlob();
        }


        internal void AddField( BlobField field )
        {
            Fields.Add( field );
        }

        BlobField LookupField( string descriptor )
        {
            foreach ( BlobField field in Fields )
            {
                string fdesc = field.GetStringDescriptor();
                if ( fdesc != null && fdesc.Equals( descriptor, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return field;
                }
            }

            return null;
        }
        BlobField LookupField( int descriptor )
        {
            foreach ( BlobField field in Fields )
            {
                if ( field.GetIntDescriptor() == descriptor )
                {
                    return field;
                }
            }

            return null;
        }
    }

}