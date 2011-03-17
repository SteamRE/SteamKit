/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamKit2
{
    public sealed class ClientTGT
    {
        public byte[] AccountRecordKey;

        public SteamGlobalUserID UserID;

        public IPAddrPort Server1;
        public IPAddrPort Server2;

        public MicroTime CreationTime;
        public MicroTime ExpirationTime;


        public static ClientTGT Deserialize( byte[] data )
        {
            DataStream ds = new DataStream( data );
            ClientTGT tgt = new ClientTGT();

            tgt.AccountRecordKey = ds.ReadBytes( 16 );

            tgt.UserID = SteamGlobalUserID.Deserialize( ds.ReadBytes( 10 ) );

            tgt.Server1 = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );
            tgt.Server2 = IPAddrPort.Deserialize( ds.ReadBytes( 6 ) );

            tgt.CreationTime = MicroTime.Deserialize( ds.ReadBytes( 8 ) );
            tgt.ExpirationTime = MicroTime.Deserialize( ds.ReadBytes( 8 ) );

            return tgt;
        }
    }
}
