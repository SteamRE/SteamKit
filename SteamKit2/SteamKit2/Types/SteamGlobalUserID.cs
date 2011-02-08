/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit2
{
    public class SteamGlobalUserID
    {
        public ushort Instance;
        public ulong AccountID;

        public SteamGlobalUserID( ushort instance, ulong accountid )
        {
            Instance = instance;
            AccountID = accountid;
        }

        public static SteamGlobalUserID Deserialize( byte[] buffer )
        {
            return SteamGlobalUserID.Deserialize( buffer, 0 );
        }
        public static SteamGlobalUserID Deserialize( byte[] buffer, int offset )
        {
            return new SteamGlobalUserID(
                BitConverter.ToUInt16( buffer, offset + 0 ),
                BitConverter.ToUInt64( buffer, offset + 2 )
            );
        }
    }
}
