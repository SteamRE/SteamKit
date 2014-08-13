/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using SteamKit2.GC;
using SteamKit2.GC.Dota.Internal;
using SteamKit2.GC.Internal;
using SteamKit2.Internal;
using ProtoBuf;

namespace SteamKit2
{
    /// <summary>
    /// This handler handles all Dota 2 GC lobbies interaction.
    /// </summary>
    public sealed partial class DotaGCHandler : ClientMsgHandler
    {
        private List<CMsgSerializedSOCache> cache = new List<CMsgSerializedSOCache>(); 

		/// <summary>
		/// The current up to date lobby
		/// </summary>
		/// <value>The lobby.</value>
		public CSODOTALobby Lobby {
			get;
			private set;
		}

        internal DotaGCHandler()
        {
                        
        }

        /// <summary>
        /// Sends a game coordinator message for a specific appid.
        /// </summary>
        /// <param name="msg">The GC message to send.</param>
        /// <param name="appId">The app id of the game coordinator to send to.</param>
        public void Send(IClientGCMsg msg, uint appId)
        {
            var clientMsg = new ClientMsgProtobuf<CMsgGCClient>(EMsg.ClientToGC);

            clientMsg.Body.msgtype = MsgUtil.MakeGCMsg(msg.MsgType, msg.IsProto);
            clientMsg.Body.appid = appId;

            clientMsg.Body.payload = msg.Serialize();

            this.Client.Send(clientMsg);
        }

        public void LaunchDota()
        {
            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            playGame.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(570),
            });

            // send it off
            // notice here we're sending this message directly using the SteamClient
            this.Client.Send(playGame);

            Thread.Sleep(5000);

            // inform the dota GC that we want a session
            var clientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
            Send(clientHello, 570);
        }

        public void CloseDota()
        {
            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            this.Client.Send(playGame);
        }

        public void JoinLobby(ulong lobbyId, string pass_key=null)
        {
            var joinLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoin>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoin);
			joinLobby.Body.lobby_id = lobbyId;
            joinLobby.Body.pass_key = pass_key;
            Send(joinLobby, 570);
        }

		public void LeaveLobby()
		{
			var leaveLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave> ((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
			this.Lobby = null;
			Send(leaveLobby, 570);
		}

        public void Pong()
        {
            var pingResponse = new ClientGCMsgProtobuf<CMsgGCClientPing> ((uint) EGCBaseClientMsg.k_EMsgGCPingResponse);
            Send(pingResponse, 570);
        }
        /// <summary>
        /// Joins a broadcast channel in the lobby
        /// </summary>
        /// <param name="channel">The channel slot to join. Valid channel values range from 0 to 5.</param>
        public void JoinBroadcastChannel(uint channel = 0)
        {
            var joinChannel = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoinBroadcastChannel> ((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoinBroadcastChannel);
            joinChannel.Body.channel = channel;
            Send(joinChannel, 570);
        }
        /// <summary>
        /// Joins a chat channel
        /// </summary>
        /// <param name="name"></param>
		public void JoinChatChannel(string name, DOTAChatChannelType_t type=DOTAChatChannelType_t.DOTAChannelType_Custom){
            var joinChannel = new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannel>((uint)EDOTAGCMsg.k_EMsgGCJoinChatChannel);
            joinChannel.Body.channel_name = name;
            joinChannel.Body.channel_type = type;
            Send(joinChannel, 570);
        }

        /// <summary>
        /// Sends a message in a chat channel.
        /// </summary>
        /// <param name="channelid">Id of channel to join.</param>
        /// <param name="message">Message to send.</param>
        public void SendChannelMessage(ulong channelid, string message)
        {
            var chatMsg = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>((uint)EDOTAGCMsg.k_EMsgGCChatMessage);
            chatMsg.Body.channel_id = channelid;
            chatMsg.Body.text = message;
            Send(chatMsg, 570);
        }
        /// <summary>
        /// Leaves chat channel
        /// </summary>
        /// <param name="channelid">id of channel to leave</param>
        public void LeaveChatChannel(ulong channelid)
        {
            var leaveChannel = new ClientGCMsgProtobuf<CMsgDOTALeaveChatChannel>((uint)EDOTAGCMsg.k_EMsgGCLeaveChatChannel);
            leaveChannel.Body.channel_id = channelid;
            Send(leaveChannel, 570);
        }
		/// <summary>
		/// Requests a lobby list with an optional password 
		/// </summary>
		/// <param name="pass_key">Pass key.</param>
		/// <param name="tournament"> Tournament games? </param>
		public void PracticeLobbyList(string pass_key=null, bool tournament=false)
        {
            var list = new ClientGCMsgProtobuf<CMsgPracticeLobbyList>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyList);
            list.Body.pass_key = pass_key;
			list.Body.tournament_games = tournament;
            Send(list, 570);
        }

        private static IPacketGCMsg GetPacketGCMsg(uint eMsg, byte[] data)
        {
            // strip off the protobuf flag
            uint realEMsg = MsgUtil.GetGCMsg(eMsg);

            if (MsgUtil.IsProtoBuf(eMsg))
            {
                return new PacketClientGCMsgProtobuf(realEMsg, data);
            }
            else
            {
                return new PacketClientGCMsg(realEMsg, data);
            }
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg(IPacketMsg packetMsg)
        {
            if (packetMsg.MsgType == EMsg.ClientFromGC)
            {
                var msg = new ClientMsgProtobuf<CMsgGCClient>(packetMsg);

                if (msg.Body.appid == 570)
                {
                    var gcmsg = GetPacketGCMsg(msg.Body.msgtype, msg.Body.payload);
                    var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
                                     {
                                         {(uint) EGCBaseClientMsg.k_EMsgGCClientWelcome, HandleWelcome},
                                         {(uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoinResponse, HandlePracticeLobbyJoinResponse},
                                         {(uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyListResponse, HandlePracticeLobbyListResponse},
                                         {(uint) ESOMsg.k_ESOMsg_CacheSubscribed, HandleCacheSubscribed},
                                         {(uint) ESOMsg.k_ESOMsg_CacheUnsubscribed, HandleCacheUnsubscribed},
                                         {(uint) EGCBaseClientMsg.k_EMsgGCPingRequest, HandlePingRequest},
                                         {(uint) EDOTAGCMsg.k_EMsgGCJoinChatChannelResponse, HandleJoinChatChannelResponse},
                                         {(uint) EDOTAGCMsg.k_EMsgGCChatMessage, HandleChatMessage},
                                         {(uint) EDOTAGCMsg.k_EMsgGCOtherJoinedChannel, HandleOtherJoinedChannel},
                                         {(uint) EDOTAGCMsg.k_EMsgGCOtherLeftChannel, HandleOtherLeftChannel},
                                         {(uint) ESOMsg.k_ESOMsg_UpdateMultiple, HandleUpdateMultiple},
                                         {(uint) EDOTAGCMsg.k_EMsgGCPopup, HandlePopup},
                                         {(uint) EDOTAGCMsg.k_EMsgDOTALiveLeagueGameUpdate, HandleLiveLeageGameUpdate}
                                     };

                    Action<IPacketGCMsg> func;
                    if (!messageMap.TryGetValue(gcmsg.MsgType, out func))
                    {
                        this.Client.PostCallback(new UnhandledDotaGCCallback(gcmsg));
                        return;
                    }

                    func(gcmsg);
                }
            }
        }

        private void HandleCacheSubscribed(IPacketGCMsg obj)
        {
			var sub = new ClientGCMsgProtobuf<CMsgSOCacheSubscribed>(obj);
			foreach(var cache in sub.Body.objects){
				if (cache.type_id == 2004) {
					HandleLobbySnapshot (cache.object_data [0]);
				}
            }
        }

        private void HandleCacheUnsubscribed(IPacketGCMsg obj)
        {
            var unSub = new ClientGCMsgProtobuf<CMsgSOCacheUnsubscribed>(obj);
			if (this.Lobby != null && unSub.Body.owner_soid.id == Lobby.lobby_id)
				this.Lobby = null;
            this.Client.PostCallback(new CacheUnsubscribed(unSub.Body));
        }

		private void HandleLobbySnapshot(byte[] data, bool update=false)
		{
			using (var stream = new MemoryStream (data)) {
				var lob = Serializer.Deserialize<CSODOTALobby> (stream);
				var oldLob = this.Lobby;
				this.Lobby = lob;
				if (update)
					this.Client.PostCallback (new PracticeLobbyUpdate (lob, oldLob));
				else
					this.Client.PostCallback (new PracticeLobbySnapshot (lob));
			}
		}

        private void HandlePracticeLobbyListResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgPracticeLobbyListResponse>(obj);
            this.Client.PostCallback(new PracticeLobbyListResponse(resp.Body));
        }

        private void HandlePracticeLobbyJoinResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoinResponse>(obj);
            this.Client.PostCallback(new PracticeLobbyJoinResponse(resp.Body));
        }
        private void HandlePingRequest(IPacketGCMsg obj)
        {
            var req = new ClientGCMsgProtobuf<CMsgGCClientPing>(obj);
            this.Client.PostCallback(new PingRequest(req.Body));
        }
        private void HandleJoinChatChannelResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAJoinChatChannelResponse>(obj);
            this.Client.PostCallback(new JoinChatChannelResponse(resp.Body));
        }
        private void HandleChatMessage(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAChatMessage>(obj);
            this.Client.PostCallback(new ChatMessage(resp.Body));
        }
        private void HandleOtherJoinedChannel(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAOtherJoinedChatChannel>(obj);
            this.Client.PostCallback(new OtherJoinedChannel(resp.Body));
        }
        private void HandleOtherLeftChannel(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAOtherLeftChatChannel>(obj);
            this.Client.PostCallback(new OtherLeftChannel(resp.Body));
        }
        private void HandleUpdateMultiple(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgSOMultipleObjects>(obj);
			var handled = true;
            foreach (var mObj in resp.Body.objects_modified)
	        {
				if (mObj.type_id == 2004) {
					HandleLobbySnapshot (mObj.object_data, true);
				} else {
					handled = false;
				}
	        }
			if (!handled) {
				this.Client.PostCallback (new UnhandledDotaGCCallback (obj));
			}
        }
        private void HandlePopup(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTAPopup>(obj);
            this.Client.PostCallback(new Popup(resp.Body));
        }
        /// <summary>
        /// GC tells us if there are tournaments running.
        /// </summary>
        /// <param name="obj"></param>
        private void HandleLiveLeageGameUpdate(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgDOTALiveLeagueGameUpdate>(obj);
            this.Client.PostCallback(new LiveLeagueGameUpdate(resp.Body));
        }
        //Initial message sent when connected to the GC
        private void HandleWelcome(IPacketGCMsg msg)
        {
            var wel = new ClientGCMsgProtobuf<CMsgClientWelcome>(msg);
            this.Client.PostCallback(new GCWelcomeCallback(wel.Body));
        }
    }
}
