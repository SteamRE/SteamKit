using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit3
{
    public partial class SteamUser
    {
        [Job( JobType = JobType.ClientJob )]
        class LogonJob : ClientJob
        {
            SteamUser.LogOnDetails logonDetails;


            public LogonJob( SteamClient client, SteamUser.LogOnDetails details )
                : base( client )
            {
                logonDetails = details;
            }


            protected async override Task YieldingRunJob( object param )
            {
                var logonMsg = new ClientMsgProtobuf<CMsgClientLogon>( EMsg.ClientLogon );

                SteamID steamId = new SteamID();

                steamId.CreateBlankUserLogon( Client.ConnectedUniverse );

                logonMsg.ProtoHeader.client_steam_id = steamId.ConvertToUint64();

                logonMsg.Body.account_name = logonDetails.Username;
                logonMsg.Body.password = logonDetails.Password;

                logonMsg.Body.protocol_version = 65571; // todo: move this out to a constant somewhere

                SendMessage( logonMsg );


                var msg = await YieldingSendMsgAndWaitForMsg( logonMsg, EMsg.ClientLogOnResponse );

                if ( msg == null )
                {
                    Client.Disconnect();
                    return;
                }

                var logonResponse = new ClientMsgProtobuf<CMsgClientLogonResponse>( msg );

#if STATIC_CALLBACKS
                var callback = new SteamUser.LoggedOnCallback( Client, logonResponse.Body );
#else
                var callback = new SteamUser.LoggedOnCallback( logonResponse.Body );
#endif

                Log.InfoFormat( "ClientLogonResponse: {0} {1}", callback.Result, callback.ExtendedResult );

                if ( callback.Result == EResult.OK )
                {
                    Client.SessionID = logonResponse.ProtoHeader.client_session_id;
                    Client.SteamID = logonResponse.Body.client_supplied_steamid;
                }
#if STATIC_CALLBACKS
                SteamClient.PostCallback( callback );
#else
                Client.PostCallback( callback );
#endif
            }
        }
    }
}
