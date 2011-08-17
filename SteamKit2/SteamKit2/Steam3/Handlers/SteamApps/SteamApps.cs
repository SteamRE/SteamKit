/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SteamKit2
{
    public sealed partial class SteamApps : ClientMsgHandler
    {

        internal SteamApps()
        {
        }


        public void GetAppOwnershipTicket( uint appid )
        {
            var request = new ClientMsgProtobuf<MsgClientGetAppOwnershipTicket>();

            request.Msg.Proto.app_id = appid;

            this.Client.Send( request );
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="e">The <see cref="SteamKit2.ClientMsgEventArgs"/> instance containing the event data.</param>
        public override void HandleMsg( ClientMsgEventArgs e )
        {
            switch ( e.EMsg )
            {
                case EMsg.ClientLicenseList:
                    HandleLicenseList( e );
                    break;

                case EMsg.ClientGameConnectTokens:
                    HandleGameConnectTokens( e );
                    break;

                case EMsg.ClientGetAppOwnershipTicketResponse:
                    HandleAppOwnershipTicketResponse( e );
                    break;

            }
        }



        #region ClientMsg Handlers
        void HandleAppOwnershipTicketResponse( ClientMsgEventArgs e )
        {
            var ticketResponse = new ClientMsgProtobuf<MsgClientGetAppOwnershipTicketResponse>();

            try
            {
                ticketResponse.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamApps", "HandleAppOwnershipTicketResponse encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            var callback = new AppOwnershipTicketCallback( Client, ticketResponse.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new AppOwnershipTicketCallback( ticketResponse.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleGameConnectTokens( ClientMsgEventArgs e )
        {
            var gcTokens = new ClientMsgProtobuf<MsgClientGameConnectTokens>( e.Data );

#if STATIC_CALLBACKS
            var callback = new GameConnectTokensCallback( Client, gcTokens.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new GameConnectTokensCallback( gcTokens.Msg.Proto );
            this.Client.PostCallback( callback );
#endif
        }
        void HandleLicenseList( ClientMsgEventArgs e )
        {
            var licenseList = new ClientMsgProtobuf<MsgClientLicenseList>();

            try
            {
                licenseList.SetData( e.Data );
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "SteamApps", "HandleLicenseList encountered an exception while reading client msg.\n{0}", ex.ToString() );
                return;
            }

#if STATIC_CALLBACKS
            var callback = new LicenseListCallback( Client, licenseList.Msg.Proto );
            SteamClient.PostCallback( callback );
#else
            var callback = new LicenseListCallback( licenseList.Msg.Proto );
            this.Client.PostCallback( callback );
#endif

        }
        #endregion


    }
}
