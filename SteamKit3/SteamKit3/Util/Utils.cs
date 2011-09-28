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

        public static EOSType GetOSType()
        {
            var osVer = Environment.OSVersion;
            var ver = osVer.Version;

            switch ( osVer.Platform )
            {
                case PlatformID.Win32Windows:
                    {
                        switch ( ver.Minor )
                        {
                            case 0:
                                return EOSType.Win95;

                            case 10:
                                return EOSType.Win98;

                            case 90:
                                return EOSType.WinME;

                            default:
                                return EOSType.Windows;
                        }
                    }

                case PlatformID.Win32NT:
                    {
                        switch ( ver.Major )
                        {
                            case 4:
                                return EOSType.WinNT;

                            case 5: // XP family
                                if ( ver.Minor == 0 )
                                    return EOSType.Win200;

                                if ( ver.Minor == 1 || ver.Minor == 2 )
                                    return EOSType.WinXP;

                                goto default;

                            case 6: // Vista & 7
                                if ( ver.Minor == 0 )
                                    return EOSType.WinVista;

                                if ( ver.Minor == 1 )
                                    return EOSType.Win7;

                                goto default;

                            default:
                                return EOSType.Windows;
                        }
                    }

                case PlatformID.Unix:
                    return EOSType.Linux; // this _could_ be mac, but we're gonna just go with linux for now

                default:
                    return EOSType.Unknown;
            }
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
