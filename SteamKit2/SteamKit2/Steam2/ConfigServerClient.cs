/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SteamKit2
{
    public class ConfigServerClient : ServerClient
    {
        public ConfigServerClient()
        {
        }

        public byte[] GetContentDescriptionRecord( byte[] oldCDRHash )
        {
            try
            {
                if ( !this.HandshakeServer( EServerType.ConfigServer ) )
                    return null;

                uint externalIp = Socket.Reader.ReadUInt32();

                if ( oldCDRHash == null )
                    oldCDRHash = new byte[ 20 ];

                if ( !SendCommand( 9, oldCDRHash ) )
                {
                    return null;
                }

                byte[] unk = Socket.Reader.ReadBytes( 11 );

                TcpPacket pack = Socket.ReceivePacket();
                Socket.Disconnect();

                if ( pack == null )
                    return null;

                return pack.GetPayload();
            }
            catch ( Exception ex )
            {
                DebugLog.WriteLine( "ConfigServerClient", "GetContentDescriptionRecord threw an exception.\n{0}", ex.ToString() );

                Socket.Disconnect();
                return null;
            }

        }
    }
}
