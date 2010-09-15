using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamKit
{
    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    public class SteamGlobalUserID : Serializable<SteamGlobalUserID>
    {
        public ushort Instance;
        public ulong AccountID;
    }
}
