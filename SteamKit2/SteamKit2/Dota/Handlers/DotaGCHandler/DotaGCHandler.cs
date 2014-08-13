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
			Send (leaveLobby, 570);
		}

		/// <summary>
		/// Requests a lobby list with an optional password 
		/// </summary>
		/// <param name="pass_key">Pass key.</param>
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
                                         {(uint) ESOMsg.k_ESOMsg_CacheSubscribed, HandleCacheSubscribed}
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
			Console.WriteLine ("Cache subscribed, Owner SOID: {0}, Type: {1}, Service: {2}, version: {3}", sub.Body.owner_soid.id, sub.Body.owner_soid.type, sub.Body.service_id, sub.Body.version);
			foreach(var cache in sub.Body.objects){
				Console.WriteLine(string.Format("==> Object type {0}", cache.type_id));
				if (cache.type_id == 2004) {
					HandleLobbySnapshot (cache.object_data [0]);
				}
            }
        }

		private void HandleLobbySnapshot(byte[] data)
		{
			using (var stream = new MemoryStream (data)) {
				var lob = Serializer.Deserialize<CSODOTALobby> (stream);
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

        //Initial message sent when connected to the GC
        private void HandleWelcome(IPacketGCMsg msg)
        {
            var wel = new ClientGCMsgProtobuf<CMsgClientWelcome>(msg);
            this.Client.PostCallback(new GCWelcomeCallback(wel.Body));
        }
    }
}
