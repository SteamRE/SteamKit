/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System.IO;
using System.Net;

namespace SteamKit2
{
    class IPAddrPort
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
            using ( var ms = new MemoryStream( data ) )
            using ( var br = new BinaryReader( ms ) )
            {
                IPAddrPort ipAddr = new IPAddrPort();

                ipAddr.IPAddress = br.ReadUInt32();
                ipAddr.Port = br.ReadUInt16();

                return ipAddr;
            }
        }
    }
}
