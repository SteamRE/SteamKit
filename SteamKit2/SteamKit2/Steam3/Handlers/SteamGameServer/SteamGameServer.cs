using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public sealed partial class SteamGameServer : ClientMsgHandler
    {

        internal SteamGameServer()
        {
        }


        public void LogOn()
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            SteamID gsId = new SteamID( 0, 0, Client.ConnectedUniverse, EAccountType.AnonGameServer );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = gsId.ConvertToUint64();

            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol;

            this.Client.Send( logon );

        }


        public override void HandleMsg( ClientMsgEventArgs e )
        {
            switch ( e.EMsg )
            {
                case EMsg.GSStatusReply:
                    HandleStatusReply( e );
                    break;

                case EMsg.ClientTicketAuthComplete:
                    HandleAuthComplete( e );
                    break;
            }
        }


        #region Handlers
        void HandleStatusReply( ClientMsgEventArgs e )
        {
            var statusReply = new ClientMsgProtobuf<MsgGSStatusReply>( e.Data );

#if STATIC_CALLBACKS
            var callback = new StatusReplyCallback( Client, statusReply.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new StatusReplyCallback( statusReply.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleAuthComplete( ClientMsgEventArgs e )
        {
            var authComplete = new ClientMsgProtobuf<MsgClientTicketAuthComplete>( e.Data );

#if STATIC_CALLBACKS
            var callback = new TicketAuthCallback( Client, authComplete.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new TicketAuthCallback( authComplete.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        #endregion
    }
}
