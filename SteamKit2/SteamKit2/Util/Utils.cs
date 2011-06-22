/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Management;

namespace SteamKit2
{
    public static class Utils
    {
        public static DateTime DateTimeFromUnixTime( uint unixTime )
        {
            DateTime origin = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
            return origin.AddSeconds( unixTime );
        }

        public static byte[] GenerateMachineID()
        {
            // this is steamkit's own implementation, it doesn't match what steamclient does
            // but this should make it so individual systems can be identified

            PlatformID platform = Environment.OSVersion.Platform;

            if ( platform == PlatformID.Win32NT )
            {
                StringBuilder hwString = new StringBuilder();

                try
                {
                    ManagementClass procClass = new ManagementClass( "Win32_Processor" );
                    foreach ( var procObj in procClass.GetInstances() )
                    {
                        hwString.AppendLine( procObj[ "ProcessorID" ].ToString() );
                    }
                }
                catch { }

                try
                {
                    ManagementClass hdClass = new ManagementClass( "Win32_LogicalDisk" );
                    foreach ( var hdObj in hdClass.GetInstances() )
                    {
                        string hdType = hdObj[ "DriveType" ].ToString();

                        if ( hdType != "3" )
                            continue; // only want local disks

                        hwString.AppendLine( hdObj[ "VolumeSerialNumber" ].ToString() );
                    }
                }
                catch { }

                try
                {
                    ManagementClass moboClass = new ManagementClass( "Win32_BaseBoard" );
                    foreach ( var moboObj in moboClass.GetInstances() )
                    {
                        hwString.AppendLine( moboObj[ "SerialNumber" ].ToString() );
                    }
                }
                catch { }


                try
                {
                    return CryptoHelper.SHAHash( Encoding.ASCII.GetBytes( hwString.ToString() ) );
                }
                catch { return null; }
            }
            else
            {
                // todo: implement me!
                return null;
            }
        }

        public static T GetAttribute<T>( this Type type )
            where T : Attribute
        {
            T[] attribs = ( T[] )type.GetCustomAttributes( typeof( T ), false );

            return attribs[ 0 ];
        }
    }

    public static class MsgUtil
    {
        private static readonly uint ProtoMask = 0x80000000;
        private static readonly uint EMsgMask = ~ProtoMask;

        public static EMsg GetMsg( uint integer )
        {
            return ( EMsg )( integer & EMsgMask );
        }

        public static EMsg GetMsg( EMsg msg )
        {
            return GetMsg( ( uint )msg );
        }

        public static bool IsProtoBuf( uint integer )
        {
            return ( integer & ProtoMask ) > 0;
        }

        public static bool IsProtoBuf( EMsg msg )
        {
            return IsProtoBuf( ( uint )msg );
        }

        public static EMsg MakeMsg( EMsg msg )
        {
            return msg;
        }

        public static EMsg MakeMsg( EMsg msg, bool protobuf )
        {
            if ( protobuf )
                return ( EMsg )( ( uint )msg | ProtoMask );

            return msg;
        }
    }

    static class NetHelpers
    {
        public static IPAddress GetLocalIP(Socket activeSocket)
        {
            IPEndPoint ipEndPoint = activeSocket.LocalEndPoint as IPEndPoint;

            if ( ipEndPoint == null || ipEndPoint.Address == IPAddress.Any )
                throw new Exception( "Socket not connected" );

            return ipEndPoint.Address;
        }

        public static IPAddress GetIPAddress( uint ipAddr )
        {
            return new IPAddress( BitConverter.GetBytes( ipAddr ) );
        }
        public static uint GetIPAddress( IPAddress ipAddr )
        {
            return BitConverter.ToUInt32( ipAddr.GetAddressBytes(), 0 );
        }


        public static uint EndianSwap( uint input )
        {
            return ( uint )IPAddress.NetworkToHostOrder( ( int )input );
        }
        public static ulong EndianSwap( ulong input )
        {
            return ( ulong )IPAddress.NetworkToHostOrder( ( long )input );
        }
        public static ushort EndianSwap( ushort input )
        {
            return ( ushort )IPAddress.NetworkToHostOrder( ( short )input );
        }
    }
}
