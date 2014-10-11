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

		/// <summary>
		/// When joining a lobby a snapshot is sent out.
		/// </summary>
		public sealed class PracticeLobbySnapshot : CallbackMsg
		{
			public CSODOTALobby lobby;

			internal PracticeLobbySnapshot(CSODOTALobby msg)
			{
				this.lobby = msg;
			}
		}

		/// <summary>
		/// Lobby was updated 
		/// </summary>
		public sealed class PracticeLobbyUpdate : CallbackMsg
		{
			public CSODOTALobby lobby;
			public CSODOTALobby oldLobby;

			internal PracticeLobbyUpdate(CSODOTALobby msg, CSODOTALobby oldLob)
			{
				this.lobby = msg;
				this.oldLobby = oldLob;
			}
		}

        /// <summary>
        /// Ping request from GC
        /// </summary>
        public sealed class PingRequest : CallbackMsg
        {
            public CMsgGCClientPing request;

            internal PingRequest(CMsgGCClientPing msg)
            {
                this.request = msg;
            }
        }
        /// <summary>
        /// Chat message received from a channel
        /// </summary>
        public sealed class ChatMessage : CallbackMsg
        {
            public CMsgDOTAChatMessage result;
            internal ChatMessage(CMsgDOTAChatMessage msg)
            {
                this.result = msg;
            }
        }
        /// <summary>
        /// Match result response.
        /// </summary>
        public sealed class MatchResultResponse : CallbackMsg
        {
            public CMsgGCMatchDetailsResponse result;
            internal MatchResultResponse(CMsgGCMatchDetailsResponse msg)
            {
                this.result = msg;
            }
        }
        /// <summary>
        /// A user joined our chat channel
        /// </summary>
        public sealed class OtherJoinedChannel : CallbackMsg
        {
            public CMsgDOTAOtherJoinedChatChannel result;
            internal OtherJoinedChannel(CMsgDOTAOtherJoinedChatChannel msg)
            {
                this.result = msg;
            }
        }
        /// <summary>
        /// A user left out chat chanel
        /// </summary>
        public sealed class OtherLeftChannel : CallbackMsg
        {
            public CMsgDOTAOtherLeftChatChannel result;
            internal OtherLeftChannel(CMsgDOTAOtherLeftChatChannel msg)
            {
                this.result = msg;
            }
        }
        /// <summary>
        /// Reponse when trying to join a chat chanel
        /// </summary>
        public sealed class JoinChatChannelResponse : CallbackMsg
        {
            public CMsgDOTAJoinChatChannelResponse result;
            internal JoinChatChannelResponse(CMsgDOTAJoinChatChannelResponse msg)
            {
                this.result = msg;
            }
        }
        /// <summary>
        /// When we leave, we receive a CacheUnsubscribed message
        /// </summary>
        public sealed class CacheUnsubscribed : CallbackMsg
        {
            public CMsgSOCacheUnsubscribed result;
            internal CacheUnsubscribed(CMsgSOCacheUnsubscribed msg)
            {
                this.result = msg;
            }
        }
        /// <summary>
        /// We receive a popup. (e.g. Kicked from lobby)
        /// </summary>
        public sealed class Popup : CallbackMsg
        {
            public CMsgDOTAPopup result;
            internal Popup(CMsgDOTAPopup msg)
            {
                this.result = msg;
            }
        }
        public sealed class LiveLeagueGameUpdate : CallbackMsg
        {
            public CMsgDOTALiveLeagueGameUpdate result;
            internal LiveLeagueGameUpdate(CMsgDOTALiveLeagueGameUpdate msg)
            {
                this.result = msg;
            }
        }
	}
}
