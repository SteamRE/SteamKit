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

namespace SteamKit3
{

    static class Utils
    {
        public static DateTime DateTimeFromUnix( uint unixTime )
        {
            DateTime origin = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
            return origin.AddSeconds( unixTime );
        }
    }

    static class MsgUtil
    {
        private static readonly uint ProtoMask = 0x80000000;
        private static readonly uint EMsgMask = ~ProtoMask;

        public static EMsg GetMsg( uint integer )
        {
            return ( EMsg )( integer & EMsgMask );
        }

        /*public static EGCMsg GetGCMsg( uint integer )
        {
            return ( EGCMsg )( integer & EMsgMask );
        }*/

        public static EMsg GetMsg( EMsg msg )
        {
            return GetMsg( ( uint )msg );
        }
        /*public static EGCMsg GetGCMsg( EGCMsg msg )
        {
            return GetGCMsg( ( uint )msg );
        }*/

        public static bool IsProtoBuf( uint integer )
        {
            return ( integer & ProtoMask ) > 0;
        }

        public static bool IsProtoBuf( EMsg msg )
        {
            return IsProtoBuf( ( uint )msg );
        }

        /*public static bool IsProtoBuf( EGCMsg msg )
        {
            return IsProtoBuf( ( uint )msg );
        }*/

        public static EMsg MakeMsg( EMsg msg )
        {
            return msg;
        }

        /*public static EGCMsg MakeGCMsg( EGCMsg msg )
        {
            return msg;
        }*/

        public static EMsg MakeMsg( EMsg msg, bool protobuf )
        {
            if ( protobuf )
                return ( EMsg )( ( uint )msg | ProtoMask );

            return msg;
        }

        /*public static EGCMsg MakeGCMsg( EGCMsg msg, bool protobuf )
        {
            if ( protobuf )
                return ( EGCMsg )( ( uint )msg | ProtoMask );

            return msg;
        }*/
    }
}
