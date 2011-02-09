/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using SteamKit2.Util;

namespace SteamKit2
{
    public class TcpPacket : BinaryWriterEx
    {
        public TcpPacket()
            : base( true )
        {
        }

        public byte[] GetPayload()
        {
            return this.ToArray();
        }

        public void SetPayload( byte[] payload )
        {
            this.Clear();
            this.Write( payload );
        }


        public byte[] GetData()
        {
            BinaryWriterEx bb = new BinaryWriterEx( true );

            byte[] payload = this.GetPayload();

            bb.Write( ( uint )payload.Length );
            bb.Write( payload );

            return bb.ToArray();
        }

    }
}
