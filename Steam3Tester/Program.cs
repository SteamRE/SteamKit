using System;
using System.Collections.Generic;
using System.Text;
using SteamLib;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;

namespace Steam3Tester
{
    enum ECacheState : byte
    {
        eCacheEmpty,
        eCachedMallocedPreprocessedVersion,
        eCachedMallocedPlaintextVersion,
        eCachePtrIsCopyOnWritePreprocessedVersion,
        eCachePtrIsCopyOnWritePlaintextVersion
    }
    enum EAutoPreprocessCode : byte
    {
        eAutoPreprocessCodePlaintext = ( byte )'P', // P = Plaintext
        eAutoPreprocessCodeCompressed = ( byte )'C', // C = Compressed
        eAutoPreprocessCodeEncrypted = ( byte )'E', // E = Encrypted
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class TBlobHeader : Serializable<TBlobHeader>
    {
        public ECacheState CacheState;
        public EAutoPreprocessCode AutoPreprocessCode;
        public uint SizeOfSerializedBlob;
        public uint SizeOfSpareCapacity;
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class TCompressedBlobHeader : Serializable<TCompressedBlobHeader>
    {
        public TBlobHeader Header;
        public uint DecompressedSize;
        public uint Unknown;
        public ushort CompressionLevel;
    }

    class Program
    {
        static bool HasEP( List<IPEndPoint> list, IPEndPoint endPoint )
        {
            foreach ( IPEndPoint listEP in list )
            {

                if ( listEP.Address.Address == endPoint.Address.Address && endPoint.Port == listEP.Port )
                    return true;
            }

            return false;
        }

        static IPEndPoint GetAddress( string server )
        {
            string[] split = server.Split( ':' );

            IPAddress ipAddr = IPAddress.Parse( split[ 0 ] );
            int port = int.Parse( split[ 1 ] );

            IPEndPoint endPoint = new IPEndPoint( ipAddr, port );

            return endPoint;
        }

        static void Main( string[] args )
        {
            CMInterface cmInterface = new CMInterface();

            cmInterface.ConnectToCM();

            /*
            GDSClient gdsClient = new GDSClient();
            List<IPEndPoint> fullCSDSList = new List<IPEndPoint>();

            foreach ( string gdsServer in GDSClient.GDServers )
            {
                IPEndPoint gdsEndPoint = GetAddress( gdsServer );

                foreach ( IPEndPoint csdsEndPoint in gdsClient.GetServerList( gdsEndPoint, EServerType.CSDS, null ) )
                {
                    if ( HasEP( fullCSDSList, csdsEndPoint ) )
                        continue;

                    fullCSDSList.Add( csdsEndPoint );
                }
            }


            CSDSClient csdsClient = new CSDSClient();
            List<IPEndPoint> fullCSList = new List<IPEndPoint>();

            foreach ( IPEndPoint csdsEndPoint in fullCSDSList )
            {
                IPEndPoint[] csList = csdsClient.GetServerList( csdsEndPoint, EServerType.ConfigServer, null );

                foreach ( IPEndPoint csEndPoint in csList )
                {
                    if ( HasEP( fullCSList, csEndPoint ) )
                        continue;

                    fullCSList.Add( csEndPoint );
                }
            }*/

            /*
            IPEndPoint[] csdsList = gdsClient.GetServerList( GetAddress( GDSClient.GDServers[ 0 ] ), EServerType.CSDS, null );

            List<IPEndPoint> fullCSList = new List<IPEndPoint>();

            CSDSClient csdsClient = new CSDSClient();

            foreach ( IPEndPoint csEndPoint in csdsList )
            {
            }*/


            /*
            Dictionary<EServerType, List<IPEndPoint>> serverMap = new Dictionary<EServerType, List<IPEndPoint>>();

            for ( int serverType = 0 ; serverType < 30 ; ++serverType )
            {
                List<IPEndPoint> serverList = new List<IPEndPoint>();
                serverMap.Add( ( EServerType )serverType, serverList );

                IPEndPoint[] serverEps = null;
                try
                {
                    serverEps = gdsClient.GetServerList( ( EServerType )serverType, null );
                }
                catch { }

                if ( serverEps == null )
                    serverEps = gdsClient.GetServerList( ( EServerType )serverType, "username" );

                if ( serverEps == null )
                    continue;

                foreach ( IPEndPoint ep in serverEps )
                {
                    if ( HasEP( serverList, ep ) )
                        continue;

                    serverList.Add( ep );
                }

            }*/

            /*
            Client client = new Client();

            client.ConnectToServer( authServers[ 0 ] );

            
            client.socket.Writer.Write( ( uint )0 );
            client.socket.Writer.Write( ( byte )4 );
            client.socket.Writer.Write( ( uint )0x6F01A8C0 );
            client.socket.Writer.Write( ( uint )0 );

            byte result = client.socket.Reader.ReadByte();

            if ( result != 0 )
            {
            }

            byte[] ipBytes = client.socket.Reader.ReadBytes( 4 );
            Array.Reverse( ipBytes );
            IPAddress extrnIp = new IPAddress( ipBytes );


            TcpPacket packet = new TcpPacket();

            packet.Append( ( byte )2 );
            packet.Append( ( ushort )userName.Length );
            packet.Append( userName, Encoding.ASCII );
            packet.Append( ( ushort )userName.Length );
            packet.Append( userName, Encoding.ASCII );


            byte[] packetData = packet.GetData();

            client.socket.Writer.Write( packetData );

            //client.SendCommand( 2, ( ushort )userName.Length, userName, ( ushort )userName.Length, userName );

            ulong salt = client.socket.Reader.ReadUInt64();*/

        }
    }
}