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
using SteamKit2.GC.TF2.Internal;
using SteamKit2.Internal;
using ProtoBuf;
using CMsgClientHello = SteamKit2.GC.Internal.CMsgClientHello;
using CMsgClientWelcome = SteamKit2.GC.Internal.CMsgClientWelcome;
using EGCBaseMsg = SteamKit2.GC.Internal.EGCBaseMsg;

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

        /// <summary>
        /// The current up to date party.
        /// </summary>
        public CSODOTAParty Party
        {
            get; 
            private set;
        }

        /// <summary>
        /// The active invite to the party.
        /// </summary>
        public CSODOTAPartyInvite PartyInvite
        {
            get; 
            private set; 
        }

        /// <summary>
        /// Last invitation to the game.
        /// </summary>
        public CMsgClientUDSInviteToGame Invitation
        {
            get; private set;
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

        /// <summary>
        /// Abandon the current game
        /// </summary>
        public void AbandonGame()
        {
            var abandon = new ClientGCMsgProtobuf<CMsgAbandonCurrentGame>((uint)EDOTAGCMsg.k_EMsgGCAbandonCurrentGame);
            Send(abandon, 570);
        }

        /// <summary>
        /// Cancel the queue for a match
        /// </summary>
        public void StopQueue()
        {
            var queue = new ClientGCMsgProtobuf<CMsgStopFindingMatch>((uint) EDOTAGCMsg.k_EMsgGCStopFindingMatch);
            Send(queue, 570);
        }

        /// <summary>
        /// Respond to a party invite
        /// </summary>
        /// <param name="party_id"></param>
        /// <param name="accept"></param>
        public void RespondPartyInvite(ulong party_id, bool accept=true)
        {
            var invite = new ClientGCMsgProtobuf<SteamKit2.GC.Internal.CMsgPartyInviteResponse>((uint) EGCBaseMsg.k_EMsgGCPartyInviteResponse);
            invite.Body.party_id = party_id;
            invite.Body.accept = accept;
            invite.Body.as_coach = false;
            invite.Body.team_id = 0;
            invite.Body.game_language_enum = 1;
            invite.Body.game_language_name = "english";
            Send(invite, 570);
        }

        public void CloseDota()
        {
            var playGame = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            this.Client.Send(playGame);
        }

        /// <summary>
        /// Join a lobby given a lobby ID
        /// </summary>
        /// <param name="lobbyId"></param>
        /// <param name="pass_key"></param>
        public void JoinLobby(ulong lobbyId, string pass_key=null)
        {
            var joinLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyJoin>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyJoin);
			joinLobby.Body.lobby_id = lobbyId;
            joinLobby.Body.pass_key = pass_key;
            Send(joinLobby, 570);
        }

        /// <summary>
        /// Leave a lobby
        /// </summary>
		public void LeaveLobby()
		{
			var leaveLobby = new ClientGCMsgProtobuf<CMsgPracticeLobbyLeave> ((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLeave);
			this.Lobby = null;
			Send(leaveLobby, 570);
		}

        /// <summary>
        /// Leave a party.
        /// </summary>
        public void LeaveParty()
        {
            var leaveParty = new ClientGCMsgProtobuf<GC.Internal.CMsgLeaveParty>((uint)EGCBaseMsg.k_EMsgGCLeaveParty);
            this.Party = null;
            Send(leaveParty, 570);
        }

        /// <summary>
        /// Respond to a ping()
        /// </summary>
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
        /// Join a team
        /// </summary>
        /// <param name="channel">The channel slot to join. Valid channel values range from 0 to 5.</param>
        public void JoinCoachSlot(DOTA_GC_TEAM team=DOTA_GC_TEAM.DOTA_GC_TEAM_GOOD_GUYS)
        {
            var joinChannel = new ClientGCMsgProtobuf<CMsgPracticeLobbySetCoach>((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbySetCoach)
            {
                Body = {team = team}
            };
            Send(joinChannel, 570);
        }

        public void RequestSubscriptionRefresh(uint type, ulong id)
        {
            var refresh =
                new ClientGCMsgProtobuf<CMsgSOCacheSubscriptionRefresh>((uint) ESOMsg.k_ESOMsg_CacheSubscriptionRefresh);
            refresh.Body.owner_soid = new CMsgSOIDOwner()
            {
                id = id,
                type = type
            };
            Send(refresh, 570);
        }

		public void JoinTeam(DOTA_GC_TEAM team, uint slot=1)
		{
			var joinSlot = new ClientGCMsgProtobuf<CMsgPracticeLobbySetTeamSlot> ((uint)EDOTAGCMsg.k_EMsgGCPracticeLobbySetTeamSlot);
			joinSlot.Body.team = team;
			joinSlot.Body.slot = slot;
			Send (joinSlot, 570);
		}

        /// <summary>
        /// Start the game
        /// </summary>
        public void LaunchLobby()
        {
            var start =
                new ClientGCMsgProtobuf<CMsgPracticeLobbyLaunch>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyLaunch);
            Send(start, 570);
        }

        /// <summary>
        /// Create a practice or tournament or custom lobby.
        /// </summary>
        /// <param name="pass_key">Password for the lobby.</param>
        /// <param name="details">Lobby options.</param>
        /// <param name="tournament_game">Is this a tournament game?</param>
        /// <param name="tournament">Tournament ID</param>
        /// <param name="tournament_game_id">Tournament game ID</param>
        public void CreateLobby(string pass_key, CMsgPracticeLobbySetDetails details, bool tournament_game=false, uint tournament=0, uint tournament_game_id=0)
        {
            var create = new ClientGCMsgProtobuf<CMsgPracticeLobbyCreate>((uint) EDOTAGCMsg.k_EMsgGCPracticeLobbyCreate);
            create.Body.pass_key = pass_key;
            create.Body.tournament_game_id = tournament_game_id;
            create.Body.tournament_game = tournament_game;
            create.Body.tournament_id = tournament;
            create.Body.lobby_details = details;
            create.Body.lobby_details.pass_key = pass_key;
            Send(create, 570);
        }

        /// <summary>
        /// Invite someone to the party.
        /// </summary>
        /// <param name="steam_id">Steam ID</param>
        public void InviteToParty(ulong steam_id)
        {
            var invite = new ClientGCMsgProtobuf<GC.Internal.CMsgInviteToParty>((uint) EGCBaseMsg.k_EMsgGCInviteToParty);
            invite.Body.steam_id = steam_id;
            Send(invite, 570);
        }

        /// <summary>
        /// Send the chat invite message.
        /// </summary>
        /// <param name="steam_id"></param>
        public void InviteToPartyUDS(ulong steam_id, ulong party_id)
        {
            var invite = new ClientMsgProtobuf<CMsgClientUDSInviteToGame>(EMsg.ClientUDSInviteToGame);
            invite.Body.connect_string = "+invite " + party_id;
            invite.Body.steam_id_dest = steam_id;
            invite.Body.steam_id_src = 0;
            this.Client.Send(invite);
        }

        /// <summary>
        /// Set coach slot in party
        /// </summary>
        /// <param name="coach"></param>
        public void SetPartyCoach(bool coach=false)
        {
            var slot =
                new ClientGCMsgProtobuf<CMsgDOTAPartyMemberSetCoach>((uint) EDOTAGCMsg.k_EMsgGCPartyMemberSetCoach);
            slot.Body.wants_coach = coach;
            Send(slot, 570);
        }

        /// <summary>
        /// Kick a player from the party
        /// </summary>
        /// <param name="steam_id">Steam ID of player to kick</param>
        public void KickPlayerFromParty(ulong steam_id)
        {
            var kick = new ClientGCMsgProtobuf<GC.Internal.CMsgKickFromParty>((uint) EGCBaseMsg.k_EMsgGCKickFromParty);
            kick.Body.steam_id = steam_id;
            Send(kick, 570);
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
        /// Request a match result
        /// </summary>
        /// <param name="matchId">Match id</param>
        public void RequestMatchResult(ulong matchId)
        {
            var requestMatch = new ClientGCMsgProtobuf<CMsgGCMatchDetailsRequest>((uint)EDOTAGCMsg.k_EMsgGCMatchDetailsRequest);
            requestMatch.Body.match_id = matchId;

            Send(requestMatch, 570);
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
                        {(uint) ESOMsg.k_ESOMsg_Destroy, HandleCacheDestroy},
                        {(uint) EGCBaseClientMsg.k_EMsgGCPingRequest, HandlePingRequest},
                        {(uint) EDOTAGCMsg.k_EMsgGCJoinChatChannelResponse, HandleJoinChatChannelResponse},
                        {(uint) EDOTAGCMsg.k_EMsgGCChatMessage, HandleChatMessage},
                        {(uint) EDOTAGCMsg.k_EMsgGCOtherJoinedChannel, HandleOtherJoinedChannel},
                        {(uint) EDOTAGCMsg.k_EMsgGCOtherLeftChannel, HandleOtherLeftChannel},
                        {(uint) ESOMsg.k_ESOMsg_UpdateMultiple, HandleUpdateMultiple},
                        {(uint) EDOTAGCMsg.k_EMsgGCPopup, HandlePopup},
                        {(uint) EDOTAGCMsg.k_EMsgDOTALiveLeagueGameUpdate, HandleLiveLeageGameUpdate},
                        {(uint) EGCBaseMsg.k_EMsgGCInvitationCreated, HandleInvitationCreated}
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
            else
            {
                if (packetMsg.IsProto && packetMsg.MsgType == EMsg.ClientUDSInviteToGame)
                {
                    var msg = new ClientMsgProtobuf<CMsgClientUDSInviteToGame>(packetMsg);
                    Invitation = msg.Body;
                    this.Client.PostCallback(new SteamPartyInvite(Invitation));
                }
            }
        }

        public void HandleInvitationCreated(IPacketGCMsg obj)
        {
            var msg = new ClientGCMsgProtobuf<GC.Internal.CMsgInvitationCreated>(obj);
            this.Client.PostCallback(new InvitationCreated(msg.Body));
        }

        private void HandleCacheSubscribed(IPacketGCMsg obj)
        {
			var sub = new ClientGCMsgProtobuf<CMsgSOCacheSubscribed>(obj);
			foreach(var cache in sub.Body.objects){
				if (cache.type_id == 2004) {
					HandleLobbySnapshot(cache.object_data [0]);
				}else if (cache.type_id == 2003)
				{
				    HandlePartySnapshot(cache.object_data[0]);
                }
                else if (cache.type_id == 2006)
                {
                    HandlePartyInviteSnapshot(cache.object_data[0]);
                }
            }
        }

        public void HandleCacheDestroy(IPacketGCMsg obj)
        {
            var dest = new ClientGCMsgProtobuf<CMsgSOSingleObject>(obj);
            if (this.PartyInvite != null && dest.Body.type_id == 2006)
            {
                this.PartyInvite = null;
                this.Client.PostCallback(new PartyInviteLeave(null));
            } 
        }

        private void HandleCacheUnsubscribed(IPacketGCMsg obj)
        {
            var unSub = new ClientGCMsgProtobuf<CMsgSOCacheUnsubscribed>(obj);
            if (this.Lobby != null && unSub.Body.owner_soid.id == Lobby.lobby_id)
            {
                this.Lobby = null;
                this.Client.PostCallback(new PracticeLobbyLeave(unSub.Body));
            }
            else if (this.Party != null && unSub.Body.owner_soid.id == Party.party_id)
            {
                this.Party = null;
                this.Client.PostCallback(new PartyLeave(unSub.Body));
            }
            else if (this.PartyInvite != null && unSub.Body.owner_soid.id == PartyInvite.group_id)
            {
                this.PartyInvite = null;
                this.Client.PostCallback(new PartyInviteLeave(unSub.Body));
            }
            else
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

        private void HandlePartySnapshot(byte[] data, bool update = false)
        {
            using (var stream = new MemoryStream(data))
            {
                var party = Serializer.Deserialize<CSODOTAParty>(stream);
                var oldParty = this.Party;
                this.Party = party;
                if (update)
                    this.Client.PostCallback(new PartyUpdate(party, oldParty));
                else
                    this.Client.PostCallback(new PartySnapshot(party));
            }
        }

        private void HandlePartyInviteSnapshot(byte[] data, bool update = false)
        {
            using (var stream = new MemoryStream(data))
            {
                var party = Serializer.Deserialize<CSODOTAPartyInvite>(stream);
                var oldParty = this.PartyInvite;
                this.PartyInvite = party;
                if (update)
                    this.Client.PostCallback(new PartyInviteUpdate(party, oldParty));
                else
                    this.Client.PostCallback(new PartyInviteSnapshot(party));
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

        private void HandleMatchResultResponse(IPacketGCMsg obj)
        {
            var resp = new ClientGCMsgProtobuf<CMsgGCMatchDetailsResponse>(obj);
            this.Client.PostCallback(new MatchResultResponse(resp.Body));
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
                }
                else if (mObj.type_id == 2003)
                {
                    HandlePartySnapshot(mObj.object_data, true);
                }
                else if (mObj.type_id == 2006)
                {
                    //HandlePartyInviteSnapshot(mObj.object_data, true);
                }
                else {
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
