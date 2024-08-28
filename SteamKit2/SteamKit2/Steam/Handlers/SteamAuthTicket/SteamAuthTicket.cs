using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.IO.Hashing;
using SteamKit2.Internal;

namespace SteamKit2
{
    /// <summary>
    /// This handler generates auth session ticket and handles it's verification by steam.
    /// </summary>
    public sealed partial class SteamAuthTicket : ClientMsgHandler
    {
        private readonly DefaultAuthTicketBuilder AuthTicketBuilder;

        private readonly Dictionary<EMsg, Action<IPacketMsg>> DispatchMap;
        private readonly ConcurrentQueue<byte[]> GameConnectTokens = new();
        private readonly ConcurrentDictionary<uint, List<CMsgAuthTicket>> TicketsByGame = new();

        /// <summary>
        /// Writes random non zero bytes into user data space of the stream.
        /// </summary>
        private class RandomUserDataSerializer : IUserDataSerializer
        {
            static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

            /// <inheritdoc/>
            public void Serialize( BinaryWriter writer )
            {
                var bytes = ArrayPool<byte>.Shared.Rent( 8 );
                try
                {
                    _random.GetNonZeroBytes( bytes );
                    writer.Write( bytes, 0, 8 );
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return( bytes );
                }
            }
        }

        /// <summary>
        /// Initializes all necessary callbacks and ticket builder.
        /// </summary>
        public SteamAuthTicket()
        {
            DispatchMap = new Dictionary<EMsg, Action<IPacketMsg>>
            {
                { EMsg.ClientAuthListAck, HandleTicketAcknowledged },
                { EMsg.ClientTicketAuthComplete, HandleTicketAuthComplete },
                { EMsg.ClientGameConnectTokens, HandleGameConnectTokens },
                { EMsg.ClientLogOff, HandleLogOffResponse }
            };
            AuthTicketBuilder = new DefaultAuthTicketBuilder( new RandomUserDataSerializer() );
        }

        /// <summary>
        /// Performs <see href="https://partner.steamgames.com/doc/api/ISteamUser#GetAuthSessionTicket">session ticket</see> generation and validation for specified <paramref name="appid"/>. 
        /// </summary>
        /// <param name="appid">Game to generate ticket for.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="TicketInfo"/> 
        /// object that provides details about the generated valid authentication session ticket.</returns>
        public async Task<TicketInfo> GetAuthSessionTicket( uint appid )
        {
            // User not logged in
            if ( Client.CellID == null ) throw new Exception( "User not logged in." );

            var apps = Client.GetHandler<SteamApps>() ?? throw new Exception( "Steam Apps instance was null." );
            var appTicket = await apps.GetAppOwnershipTicket( appid );

            // User doesn't own the game
            if ( appTicket.Result != EResult.OK ) throw new Exception( "User doesn't own the game." );

            if ( GameConnectTokens.TryDequeue( out var token ) )
            {
                var authTicket = AuthTicketBuilder.Build( token );
                var ticket = await VerifyTicket( appid, authTicket, out var crc );

                // Verify just in case
                if ( ticket.ActiveTicketsCRC.Any( x => x == crc ) )
                {
                    var tok = BuildTicket( authTicket, appTicket.Ticket );
                    return new TicketInfo( this, appid, tok );
                }
                else
                {
                    throw new Exception( "Ticket verification failed." );
                }
            }
            else
            {
                throw new Exception( "There's no available game connect tokens left." );
            }
        }

        internal void CancelAuthTicket( TicketInfo authTicket )
        {

            if ( TicketsByGame.TryGetValue( authTicket.AppID, out var tickets ) )
            {
                tickets.RemoveAll( x => x.ticket_crc == authTicket.TicketCRC );
            }

            SendTickets();
        }

        private static byte[] BuildTicket( byte[] authTicket, byte[] appTicket )
        {
            var len = appTicket.Length;
            var token = new byte[ authTicket.Length + 4 + len ];
            var mem = token.AsSpan();
            authTicket.CopyTo( mem );
            MemoryMarshal.Write( mem[ authTicket.Length.. ], in len );
            appTicket.CopyTo( mem[ ( authTicket.Length + 4 ).. ] );

            return token;
        }

        private AsyncJob<TicketAcceptedCallback> VerifyTicket( uint appid, byte[] authToken, out uint crc )
        {
            crc = BitConverter.ToUInt32( Crc32.Hash( authToken ), 0 );
            var items = TicketsByGame.GetOrAdd( appid, [] );

            // Add ticket to specified games list
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
            auth.Body.tokens_left = ( uint )GameConnectTokens.Count;

            auth.Body.app_ids.AddRange( TicketsByGame.Keys );
            // Flatten dictionary into ticket list
            auth.Body.tickets.AddRange( TicketsByGame.Values.SelectMany( x => x ) );

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
            ArgumentNullException.ThrowIfNull( packetMsg );

            if ( DispatchMap.TryGetValue( packetMsg.MsgType, out var handlerFunc ) )
            {
                handlerFunc( packetMsg );
            }
        }

        #region ClientMsg Handlers
        private void HandleLogOffResponse( IPacketMsg packetMsg )
        {
            // Clear all game connect tokens on client log off
            GameConnectTokens.Clear();
        }
        private void HandleGameConnectTokens( IPacketMsg packetMsg )
        {
            var body = new ClientMsgProtobuf<CMsgClientGameConnectTokens>( packetMsg ).Body;

            // Add tokens
            foreach ( var tok in body.tokens )
            {
                GameConnectTokens.Enqueue( tok );
            }

            // Keep only required amount, discard old entries
            while ( GameConnectTokens.Count > body.max_tokens_to_keep )
            {
                GameConnectTokens.TryDequeue( out _ );
            }
        }
        private void HandleTicketAuthComplete( IPacketMsg packetMsg )
        {
            // Ticket successfully used to authorize user
            var complete = new ClientMsgProtobuf<CMsgClientTicketAuthComplete>( packetMsg );
            var inUse = new TicketAuthCompleteCallback( complete.TargetJobID, complete.Body );
            Client.PostCallback( inUse );
        }
        private void HandleTicketAcknowledged( IPacketMsg packetMsg )
        {
            // Ticket acknowledged as valid by Steam
            var authAck = new ClientMsgProtobuf<CMsgClientAuthListAck>( packetMsg );
            var acknowledged = new TicketAcceptedCallback( authAck.TargetJobID, authAck.Body );
            Client.PostCallback( acknowledged );
        }
        #endregion
    }
}
