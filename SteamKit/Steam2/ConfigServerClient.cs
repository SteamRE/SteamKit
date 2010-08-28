using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SteamLib
{
    class ConfigServerClient : ServerClient
    {

        public byte[] GetClientConfigRecord( IPEndPoint configServer )
        {
            if ( !this.ConnectToServer( configServer ) )
                return null;

            if ( !this.HandshakeServer( EServerType.ConfigServer ) )
            {
                this.Disconnect();
                return null;
            }

            uint externalIp = socket.Reader.ReadUInt32();

            if ( !this.SendCommand( 1 ) ) // command: Get CCR
            {
                this.Disconnect();
                return null;
            }

            TcpPacket pack = this.RecvPacket();
            this.Disconnect();

            if ( pack == null )
                return null;

            return pack.GetPayload();
        }

        public byte[] GetContentDescriptionRecord( IPEndPoint endPoint, byte[] oldCDRHash )
        {
            if ( !this.ConnectToServer( endPoint ) )
                return null;

            if ( !this.HandshakeServer( EServerType.ConfigServer ) )
            {
                this.Disconnect();
                return null;
            }

            uint externalIp = socket.Reader.ReadUInt32();

            if ( oldCDRHash == null )
                oldCDRHash = new byte[ 20 ];

            if ( !SendCommand( 9, oldCDRHash ) )
            {
                this.Disconnect();
                return null;
            }

            byte[] unknown = socket.Reader.ReadBytes( 11 );

            TcpPacket packet = this.RecvPacket();
            this.Disconnect();

            if ( packet == null )
                return null;

            return packet.GetPayload();
        }
    }
}
