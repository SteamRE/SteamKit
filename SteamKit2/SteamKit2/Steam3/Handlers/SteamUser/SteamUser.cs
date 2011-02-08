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
    public class LoginKeyCallback : CallbackMsg
    {
        public byte[] LoginKey { get; set; }
        public uint UniqueID { get; set; }

        internal LoginKeyCallback( MsgClientNewLoginKey logKey )
        {
            this.LoginKey = logKey.LoginKey;
            this.UniqueID = logKey.UniqueID;
        }
    }
    public class LogOnResponseCallback : CallbackMsg
    {
        public EResult Result { get; set; }

        public int OutOfGameSecsPerHeartbeat { get; set; }
        public int InGameSecsPerHeartbeat { get; set; }

        public IPAddress PublicIP { get; set; }

        public DateTime ServerTime { get; set; }

        public EAccountFlags AccountFlags { get; set; }

        public SteamID ClientSteamID { get; set; }

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
    }

    public class SteamUser : ClientMsgHandler
    {
        public const string NAME = "SteamUser";

        public SteamUser()
            : base( SteamUser.NAME )
        {
        }


        public class LogOnDetails
        {
            public string Username;
            public string Password;

            public ClientTGT ClientTGT;
            public byte[] ServerTGT;
            public Blob AccRecord;
        }
        public void LogOn( LogOnDetails details )
        {
            var logon = new ClientMsgProtobuf<MsgClientLogon>();

            SteamID steamId = new SteamID();
            steamId.SetFromSteam2( details.ClientTGT.UserID, Client.ConnectedUniverse );
            
            // todo:
            // steam2 always gives us an instance of 0,
            // valve's steamclient seems to ignore this and use an instance of 1
            // if we use an instance of 0, we never get the NewLoginKey msg and then can't sign on to friends
            steamId.AccountInstance = 1;

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

            logon.Msg.Proto.cell_id = 10; // todo: figure out how to grab a cell id
            logon.Msg.Proto.client_package_version = 1385;

            logon.Msg.Proto.email_address = details.AccRecord.GetStringDescriptor( AuthFields.eFieldEmail );

            byte[] serverTgt = new byte[ details.ServerTGT.Length + 4 ];

            Array.Copy( BitConverter.GetBytes( localIp ), serverTgt, 4 );
            Array.Copy( details.ServerTGT, 0, serverTgt, 4, details.ServerTGT.Length );

            logon.Msg.Proto.steam2_auth_ticket = serverTgt;

            this.Client.Send( logon );
        }
        public void LogOff()
        {
            var logOff = new ClientMsgProtobuf<MsgClientLogOff>();
            this.Client.Send( logOff );
        }

        public SteamID GetSteamID()
        {
            return this.Client.SteamID;
        }


        public override void HandleMsg( ClientMsgEventArgs e )
        {
            switch ( e.EMsg )
            {
                case EMsg.ClientLogOnResponse:
                    HandleLogOnResponse( e );
                    break;

                case EMsg.ClientNewLoginKey:
                    HandleLoginKey( e );
                    break;
            }
        }


        void HandleLoginKey( ClientMsgEventArgs e )
        {
            var loginKey = new ClientMsg<MsgClientNewLoginKey, ExtendedClientMsgHdr>( e.Data );

            var resp = new ClientMsg<MsgClientNewLoginKeyAccepted, ExtendedClientMsgHdr>();
            resp.Msg.UniqueID = loginKey.Msg.UniqueID;

            this.Client.Send( resp );

            var callback = new LoginKeyCallback( loginKey.Msg );
            this.Client.PostCallback( callback );
        }
        void HandleLogOnResponse( ClientMsgEventArgs e )
        {
            if ( e.IsProto )
            {
                var logonResp = new ClientMsgProtobuf<MsgClientLogonResponse>( e.Data );

                var callback = new LogOnResponseCallback( logonResp.Msg.Proto );
                this.Client.PostCallback( callback );
            }
        }
    }
}
