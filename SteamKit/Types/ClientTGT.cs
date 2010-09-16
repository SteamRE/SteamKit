using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamKit
{
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class ClientTGT : Serializable<ClientTGT>
    {
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
        public byte[] AccountRecordKey;

        public SteamGlobalUserID UserID;

        public IPAddrPort Server1;
        public IPAddrPort Server2;

        public MicroTime CreationTime;
        public MicroTime ExpirationTime;
    }
}
