/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;

namespace SteamKit2
{
	public partial class DotaGCHandler
	{
        //The first message the GC sends after connection.
	    public sealed class GCWelcomeCallback : CallbackMsg
	    {
	        public uint Version;

	        internal GCWelcomeCallback(CMsgClientWelcome msg)
	        {
	            this.Version = msg.version;
	        }
	    }

        //Called when an unhandled message is received from the Dota 2 GC
	    public sealed class UnhandledDotaGCCallback : CallbackMsg
	    {
	        public IPacketGCMsg Message;

	        internal UnhandledDotaGCCallback(IPacketGCMsg msg)
	        {
	            Message = msg;
	        }
	    }
        
        //PracticeLobby join response
        public sealed class PracticeLobbyJoinResponse : CallbackMsg
        {
            public CMsgPracticeLobbyJoinResponse result;

            internal PracticeLobbyJoinResponse(CMsgPracticeLobbyJoinResponse msg)
            {
                this.result = msg;
            }
        }

        //PracticeLobby list response
        public sealed class PracticeLobbyListResponse : CallbackMsg
        {
            public CMsgPracticeLobbyListResponse result;

            internal PracticeLobbyListResponse(CMsgPracticeLobbyListResponse msg)
            {
                this.result = msg;
            }
        }
	}
}
