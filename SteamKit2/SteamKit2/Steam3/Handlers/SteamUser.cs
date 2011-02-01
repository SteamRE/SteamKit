using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamKit2
{
    public struct LogOnDetails
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public ClientTGT ClientTGT { get; set; }
        public byte[] ServerTGT { get; set; }
        public Blob AccRecord { get; set; }
    }

    public class SteamUser : ClientMsgHandler
    {
        public const string NAME = "SteamUser";

        public SteamUser()
            : base( SteamUser.NAME )
        {
        }

        public void LogOn( LogOnDetails details )
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            SteamID steamId = new SteamID();
            steamId.SetFromSteam2( details.ClientTGT.UserID, Client.ConnectedUniverse );

            uint localIp = NetHelpers.GetIPAddress( NetHelpers.GetLocalIP() );

            MicroTime creationTime = MicroTime.Deserialize( details.AccRecord.GetDescriptor( AuthFields.eFieldTimestampCreation ) );

            logon.ProtoHeader.client_session_id = 0;
            logon.ProtoHeader.client_steam_id = steamId.ConvertToUint64();

            logon.Msg.Proto.obfustucated_private_ip = localIp ^ MsgClientLogon.ObfuscationMask;

            logon.Msg.Proto.account_name = details.Username;
            logon.Msg.Proto.password = details.Password;

            logon.Msg.Proto.protocol_version = MsgClientLogon.CurrentProtocol;
            logon.Msg.Proto.client_os_type = 10;
            logon.Msg.Proto.client_language = "english";
            logon.Msg.Proto.rtime32_account_creation = creationTime.ToUnixTime();

            logon.Msg.Proto.cell_id = 10;
            logon.Msg.Proto.client_package_version = 1385;

            logon.Msg.Proto.email_address = details.AccRecord.GetStringDescriptor( AuthFields.eFieldEmail );

            byte[] serverTgt = new byte[ details.ServerTGT.Length + 4 ];

            Array.Copy( BitConverter.GetBytes( localIp ), serverTgt, 4 );
            Array.Copy( details.ServerTGT, 0, serverTgt, 4, details.ServerTGT.Length );

            logon.Msg.Proto.steam2_auth_ticket = serverTgt;

            Client.Send( logon );
        }

        internal override void HandleMsg( EMsg eMsg, byte[] data )
        {
            switch ( eMsg )
            {
                case EMsg.ClientLogOnResponse:
                    HandleLogOnResponse( data );
                    break;
            }
        }


        private void HandleLogOnResponse( byte[] data )
        {
            var logonResp = new ClientMsgProtobuf<MsgClientLogonResponse>( data );
        }


    }
}
