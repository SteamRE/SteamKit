using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit2
{
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
}
