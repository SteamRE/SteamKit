using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit2
{
    public class TcpPacket : ByteBuffer
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
            this.Append( payload );
        }


        public byte[] GetData()
        {
            ByteBuffer bb = new ByteBuffer( true );

            byte[] payload = this.GetPayload();

            bb.Append( ( uint )payload.Length );
            bb.Append( payload );

            return bb.ToArray();
        }

    }
}
