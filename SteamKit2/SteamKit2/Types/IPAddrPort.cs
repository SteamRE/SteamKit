/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace SteamKit2
{
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class IPAddrPort //: Serializable<IPAddrPort>
    {
        public uint IPAddress;
        public ushort Port;



        public static implicit operator IPEndPoint( IPAddrPort addr )
        {
            return addr.ToEndPoint();
        }


        public IPAddress ToIPAddress()
        {
            return NetHelpers.GetIPAddress( NetHelpers.EndianSwap( IPAddress ) );
        }

        public IPEndPoint ToEndPoint()
        {
            return new IPEndPoint( ToIPAddress(), Port );
        }


        public override string ToString()
        {
            return ToEndPoint().ToString();
        }


        
        public static IPAddrPort Deserialize( byte[] data )
        {
            DataStream ds = new DataStream( data );

            IPAddrPort ipAddr = new IPAddrPort();

            ipAddr.IPAddress = ds.ReadUInt32();
            ipAddr.Port = ds.ReadUInt16();

            return ipAddr;
        }

        public byte[] Serialize()
        {
            using ( BinaryWriterEx bw = new BinaryWriterEx() )
            {

                bw.Write( this.IPAddress );
                bw.Write( this.Port );

                return bw.ToArray();
            }
        }
    }
}
