using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;

namespace SteamKit
{
    public enum SessionType
    {
        GameServer,
        User
    }

    public class SteamSession
    {
        public SessionType  Type  { get; private set; } 

        private String       username;
        private SecureString password;
        public  SteamID      RequestedID { get; private set; }

        // steam2
        /*
        public static ClientTGT ClientTGT       { get; private set; }
        public static byte[]    ServerTGT       { get; private set; }
        public static Blob      AccountRecord   { get; private set; }
        */

        // steam3
        public SteamID  SteamID   { get; private set; }
        public int      SessionID { get; private set; }

        private SteamSession()
        {
        }

        public SteamSession(string username, SecureString password) : this()
        {
            Type = SessionType.User;

            this.username = username;
            this.password = password;
        }

        public SteamSession(EAccountType type) : this()
        {
            Type = SessionType.GameServer;

            RequestedID = new SteamID(0, EUniverse.Public, type);
        }
    }
}
