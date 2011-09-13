/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamKit3
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

            SteamID steamId = new SteamID( 0, logonDetails.AccountInstance, Client.ConnectedUniverse, EAccountType.Individual );

            logonMsg.ProtoHeader.client_steam_id = steamId.ConvertToUint64();

            logonMsg.Body.account_name = logonDetails.Username;
            logonMsg.Body.password = logonDetails.Password;

            logonMsg.Body.protocol_version = 65571; // todo: move this out to a constant somewhere

            SendMessage( logonMsg );

            IPacketMsg msg = await YieldingWaitForMsg( EMsg.ClientLogOnResponse );
            var logonResponse = new ClientMsgProtobuf<CMsgClientLogonResponse>( msg );

            // todo: post callback, etc
            Console.WriteLine( "Logon response: {0}", ( EResult )logonResponse.Body.eresult );
        }
    }
}