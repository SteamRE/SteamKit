using System;

namespace SteamKit
{
    public class SteamGlobalUserID
    {
        public static readonly int Size = 6;

        public ushort   Instance;
        public ulong    AccountID;

        public SteamGlobalUserID(ushort instance, ulong accountid)
        {
            Instance = instance;
            AccountID = accountid;
        }

        public static SteamGlobalUserID Deserialize(byte[] buffer)
        {
            return new SteamGlobalUserID(
                                        BitConverter.ToUInt16(buffer, 0),
                                        BitConverter.ToUInt64(buffer, 2)
                                        );
        }
    }
}
