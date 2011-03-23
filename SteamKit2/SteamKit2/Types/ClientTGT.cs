/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace SteamKit2
{

    /// <summary>
    /// Represents a client readable Ticket Granting Ticket (TGT) from the Steam2 network
    /// </summary>
    public sealed class ClientTGT
    {
        /// <summary>
        /// Gets the 128 bit AES key used when decrypting the account record blob.
        /// </summary>
        /// <value>128 bit AES key.</value>
        public byte[] AccountRecordKey { get; set; }

        /// <summary>
        /// Gets the Steam2 UserID.
        /// </summary>
        /// <value>The UserID.</value>
        public SteamGlobalUserID UserID { get; set; }

        public IPAddrPort Server1 { get; set; }
        public IPAddrPort Server2 { get; set; }

        /// <summary>
        /// Gets the creation time of this TGT.
        /// </summary>
        /// <value>The creation time.</value>
        public MicroTime CreationTime { get; set; }
        /// <summary>
        /// Gets the expiration time of this TGT.
        /// </summary>
        /// <value>The expiration time.</value>
        public MicroTime ExpirationTime { get; set; }


        /// <summary>
        /// Deserializes a ClientTGT from a block of data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A ClientTGT.</returns>
        public static ClientTGT Deserialize( byte[] data )
        {
            using ( MemoryStream ms = new MemoryStream( data ) )
            using ( BinaryReader br = new BinaryReader( ms ) )
            {
                ClientTGT tgt = new ClientTGT();

                tgt.AccountRecordKey = br.ReadBytes( 16 );

                tgt.UserID = SteamGlobalUserID.Deserialize( br.ReadBytes( 10 ) );

                tgt.Server1 = IPAddrPort.Deserialize( br.ReadBytes( 6 ) );
                tgt.Server2 = IPAddrPort.Deserialize( br.ReadBytes( 6 ) );

                tgt.CreationTime = MicroTime.Deserialize( br.ReadBytes( 8 ) );
                tgt.ExpirationTime = MicroTime.Deserialize( br.ReadBytes( 8 ) );

                return tgt;
            }
        }
    }
}
