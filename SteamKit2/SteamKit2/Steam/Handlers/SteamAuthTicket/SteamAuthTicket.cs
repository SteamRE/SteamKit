using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler generates auth session ticket and handles it's verification by steam.
    /// </summary>
    public sealed partial class SteamAuthTicket : ClientMsgHandler
    {
        private readonly Dictionary<EMsg, Action<IPacketMsg>> dispatchMap;
        private readonly ConcurrentQueue<byte[]> gameConnectTokens = new ConcurrentQueue<byte[]>();
        private readonly ConcurrentDictionary<uint, List<CMsgAuthTicket>> ticketsByGame = new ConcurrentDictionary<uint, List<CMsgAuthTicket>>();

        internal SteamAuthTicket()
        {
            dispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientAuthListAck, HandleTicketAcknowledged },
                { EMsg.ClientTicketAuthComplete, HandleTicketAuthComplete },
                { EMsg.ClientGameConnectTokens, HandleGameConnectTokens },
                { EMsg.ClientLogOnResponse, HandleLogOnResponse }
            };
        }

        /// <summary>
        /// Generate session ticket, and verify it with steam servers.
        /// </summary>
        /// <param name="appid">The appid to request the ticket of.</param>
        /// <returns><c>null</c> if user isn't fully logged in, doesn't own the game, or steam deemed ticket invalid; otherwise <see cref="TicketInfo" /> instance.</returns>
        public async Task<TicketInfo?> GetAuthSessionTicket( uint appid )
        {
            // not logged in
            if ( Client.CellID == null )
            {
                return null;
            }

            var apps = Client.GetHandler<SteamApps>()!;
            var appTicket = await apps.GetAppOwnershipTicket( appid );
            // user doesn't own the game
            if ( appTicket.Result != EResult.OK )
            {
                return null;
            }

            if ( gameConnectTokens.TryDequeue( out var token ) )
            {
                byte[] authToken = CreateAuthToken( token );
                var ticketTask = await VerifyTicket( appid, authToken, out var crc );
                // verify the ticket is on the list of accepted tickets
                // didn't happen on my testing, but I don't think it hurts to check
                if ( ticketTask.ActiveTicketsCRC.Any( x => x == crc ) )
                {
                    return new TicketInfo( this, appid, crc, BuildTicket( authToken, appTicket.Ticket ) );
                }
            }
            return null;
        }
        internal bool CancelAuthTicket( TicketInfo ticket )
        {
            if(ticketsByGame.TryGetValue(ticket.AppID, out var values))
            {
                if ( values.RemoveAll( x => x.ticket_crc == ticket.CRC ) > 0 )
                {
                    SendTickets();
                }
            }
            return false;
        }

        private byte[] BuildTicket( byte[] authToken, byte[] appTicket )
        {
            using ( var stream = new MemoryStream( authToken.Length + 4 + appTicket.Length ) )
            {
                using ( var writer = new BinaryWriter( stream ) )
                {
                    writer.Write( authToken );
                    writer.Write( appTicket.Length );
                    writer.Write( appTicket );
                }
                return stream.ToArray();
            }
        }
        private byte[] CreateAuthToken( byte[] gameConnectToken )
        {
            const int sessionSize =
                    4 + // unknown 1
                    4 + // unknown 2
                    4 + // external IP
                    4 + // padding
                    4 + // connection time
                    4;  // connection count

            // We checked that we're connected before calling this function
            uint ipAddress = NetHelpers.GetIPAddress( Client.PublicIP! );
            int connectionTime = ( int )( ( DateTime.UtcNow - serverTime ).TotalMilliseconds );
            using ( var stream = new MemoryStream( 4 + gameConnectToken.Length + 4 + sessionSize ) )
            {
                using ( var writer = new BinaryWriter( stream ) )
                {
                    writer.Write( gameConnectToken.Length );
                    writer.Write( gameConnectToken.ToArray() );

                    writer.Write( sessionSize );
                    writer.Write( 1 );
                    writer.Write( 2 );

                    writer.Write( ipAddress );
                    writer.Write( 0 ); // padding
                    writer.Write( connectionTime ); // in milliseconds
                    writer.Write( 1 ); // single client connected
                }

                return stream.ToArray();
            }
        }

        private AsyncJob<TicketAcceptedCallback> VerifyTicket( uint appid, byte[] authToken, out uint crc )
        {
            crc = Crc32.Compute( authToken );
            var items = ticketsByGame.GetOrAdd( appid, new List<CMsgAuthTicket>() );

            // add ticket to specified games list
            items.Add( new CMsgAuthTicket
            {
                gameid = appid,
                ticket = authToken,
                ticket_crc = crc
            } );
            return SendTickets();
        }
        private AsyncJob<TicketAcceptedCallback> SendTickets()
        {
            var auth = new ClientMsgProtobuf<CMsgClientAuthList>( EMsg.ClientAuthList );
            auth.Body.tokens_left = ( uint )gameConnectTokens.Count;
            // all registered games
            auth.Body.app_ids.AddRange( ticketsByGame.Keys );
            // flatten all registered per-game tickets
            auth.Body.tickets.AddRange( ticketsByGame.Values.SelectMany( x => x ) );
            auth.SourceJobID = Client.GetNextJobID();
            Client.Send( auth );
            return new AsyncJob<TicketAcceptedCallback>( Client, auth.SourceJobID );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg( IPacketMsg packetMsg )
        {
            if ( packetMsg == null )
            {
                throw new ArgumentNullException( nameof( packetMsg ) );
            }

            if ( dispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc ) )
            {
                handlerFunc( packetMsg );
            }
        }

        #region ClientMsg Handlers
        private void HandleLogOnResponse( IPacketMsg packetMsg )
        {
            var body = new ClientMsgProtobuf<CMsgClientLogonResponse>( packetMsg ).Body;
            // just grabbing server time
            serverTime = DateUtils.DateTimeFromUnixTime( body.rtime32_server_time );
        }
        private void HandleGameConnectTokens( IPacketMsg packetMsg )
        {
            var body = new ClientMsgProtobuf<CMsgClientGameConnectTokens>( packetMsg ).Body;

            // add tokens
            foreach ( var tok in body.tokens )
            {
                gameConnectTokens.Enqueue( tok );
            }

            // keep only required amount, discard old entries
            while ( gameConnectTokens.Count > body.max_tokens_to_keep )
            {
                gameConnectTokens.TryDequeue( out _ );
            }
        }
        private void HandleTicketAuthComplete( IPacketMsg packetMsg )
        {
            // ticket successfully used to authorize user
            var complete = new ClientMsgProtobuf<CMsgClientTicketAuthComplete>( packetMsg );
            var inUse = new TicketAuthCompleteCallback( complete.TargetJobID, complete.Body );
            Client.PostCallback( inUse );
        }
        private void HandleTicketAcknowledged( IPacketMsg packetMsg )
        {
            // ticket acknowledged as valid by steam
            var authAck = new ClientMsgProtobuf<CMsgClientAuthListAck>( packetMsg );
            var acknowledged = new TicketAcceptedCallback( authAck.TargetJobID, authAck.Body );
            Client.PostCallback( acknowledged );
        }
        #endregion

        private DateTime serverTime;

    }
}
