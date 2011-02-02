using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

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

    public class LogOnResponseCallback : CallbackMsg
    {
        internal LogOnResponseCallback( CMsgClientLogonResponse resp )
        {
            this.Result = ( EResult )resp.eresult;

            this.OutOfGameSecsPerHeartbeat = resp.out_of_game_heartbeat_seconds;
            this.InGameSecsPerHeartbeat = resp.in_game_heartbeat_seconds;

            this.PublicIP = NetHelpers.GetIPAddress( resp.public_ip );

            this.ServerTime = Utils.DateTimeFromUnixTime( resp.rtime32_server_time );

            this.AccountFlags = (EAccountFlags)resp.account_flags;

            this.ClientSteamID = new SteamID( resp.client_supplied_steamid );
        }

        public EResult Result { get; set; }

        public int OutOfGameSecsPerHeartbeat { get; set; }
        public int InGameSecsPerHeartbeat { get; set; }

        public IPAddress PublicIP { get; set; }

        public DateTime ServerTime { get; set; }

        public EAccountFlags AccountFlags { get; set; }

        public SteamID ClientSteamID { get; set; }
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
            logon.Msg.Proto.client_os_type = 10; // windows
            logon.Msg.Proto.client_language = "english";
            logon.Msg.Proto.rtime32_account_creation = creationTime.ToUnixTime();

            logon.Msg.Proto.cell_id = 10; // TODO: figure out how to grab a cell id
            logon.Msg.Proto.client_package_version = 1385;

            logon.Msg.Proto.email_address = details.AccRecord.GetStringDescriptor( AuthFields.eFieldEmail );

            byte[] serverTgt = new byte[ details.ServerTGT.Length + 4 ];

            Array.Copy( BitConverter.GetBytes( localIp ), serverTgt, 4 );
            Array.Copy( details.ServerTGT, 0, serverTgt, 4, details.ServerTGT.Length );

            logon.Msg.Proto.steam2_auth_ticket = serverTgt;

            Client.Send( logon );
        }

        public override void HandleMsg( EMsg eMsg, byte[] data )
        {
            switch ( eMsg )
            {
                case EMsg.ClientLogOnResponse:
                    HandleLogOnResponse( data );
                    break;
            }
        }

        void HandleLogOnResponse( byte[] data )
        {
            var logonResp = new ClientMsgProtobuf<MsgClientLogonResponse>( data );

            var callback = new LogOnResponseCallback( logonResp.Msg.Proto );
            this.Client.PostCallback( callback );
        }


    }
}
