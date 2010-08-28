using System;
using System.Collections.Generic;
using System.Text;
using BlobLib;
using System.Threading;

namespace SteamLib
{
    // so steam2 and steam3 can communicate to each other
    static class SteamGlobal
    {
        static object lockObj = new object();


        // steam2
        public static ClientTGT ClientTGT { get; set; }
        public static byte[] ServerTGT { get; set; }
        public static Blob AccountRecord { get; set; }

        // steam3
        public static SteamID SteamID { get; set; }
        public static int SessionID { get; set; }


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
