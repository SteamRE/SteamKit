using System;
using System.Collections.Generic;
using System.Text;
using BlobLib;
using System.Threading;

namespace SteamKit
{
    // so steam2 and steam3 can communicate to each other
    public static class SteamGlobal
    {
        static object lockObj = new object();

        public static String username { get; set; }
        public static String password { get; set; }

        // steam2
        public static ClientTGT ClientTGT { get; set; }
        public static byte[] ServerTGT { get; set; }
        public static Blob AccountRecord { get; set; }

        // steam3
        public static SteamID SteamID { get; set; }
        public static int SessionID { get; set; }

        public static byte[] WinUITicket { get; set; }
        public static byte[] LoginKey { get; set; }

        public static List<CMsgClientFriendsList.Friend> Friends { get; set; } // lazy!

        public static void Lock()
        {
            Monitor.Enter( lockObj );
        }
        public static void Unlock()
        {
            Monitor.Exit( lockObj );
        }
    }
}
