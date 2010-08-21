using System;
using System.Collections.Generic;
using System.Text;
using SteamLib;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

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

    class ServerList
    {
        List<IPEndPoint> servers;

        public EServerType Type { get; private set; }

        public ServerList( EServerType type )
        {
            this.Type = type;
            this.servers = new List<IPEndPoint>();
        }

        public void AddServer( IPEndPoint endPoint )
        {
            foreach ( IPEndPoint server in servers )
            {
                if ( server.Address.Equals( endPoint.Address ) && server.Port == endPoint.Port )
                    return;
            }

            servers.Add( endPoint );
        }

        public IPEndPoint[] GetServers()
        {
            return servers.ToArray();
        }
    }

    static class ServerCache
    {
        static Dictionary<EServerType, ServerList> serverMap;

        static ServerCache()
        {
            serverMap = new Dictionary<EServerType, ServerList>();
        }

        public static void AddServer( EServerType type, IPEndPoint server )
        {
            if ( !serverMap.ContainsKey( type ) )
                serverMap.Add( type, new ServerList( type ) );

            serverMap[ type ].AddServer( server );
        }

        public static void AddServers( EServerType type, IPEndPoint[] servers )
        {
            if ( servers == null )
                return;

            foreach ( IPEndPoint endPoint in servers )
                AddServer( type, endPoint );
        }

        public static ServerList GetServers( EServerType type )
        {
            if ( !serverMap.ContainsKey( type ) )
                return null;

            return serverMap[ type ];
        }

        public static ServerList[] GetServerLists()
        {
            List<ServerList> serverLists = new List<ServerList>();

            foreach ( var kvp in serverMap )
                serverLists.Add( kvp.Value );

            return serverLists.ToArray();
        }


    }


    class Program
    {

        static IPEndPoint GetAddress( string server )
        {
            string[] split = server.Split( ':' );

            IPAddress ipAddr = IPAddress.Parse( split[ 0 ] );
            int port = int.Parse( split[ 1 ] );

            IPEndPoint endPoint = new IPEndPoint( ipAddr, port );

            return endPoint;
        }

        static byte[] GenerateAESKey( uint salt1, uint salt2, string pass )
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( salt1 );
            bb.Append( Encoding.ASCII.GetBytes( pass ) );
            bb.Append( salt2 );

            byte[] digest = CryptoHelper.SHAHash( bb.ToArray() );

            byte[] aesKey = new byte[ 16 ];
            Array.Copy( digest, 0, aesKey, 0, 16 );

            return aesKey;
        }

        static ulong GetObfuscationMask( uint internalIp, uint externalIp )
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( externalIp );
            bb.Append( internalIp );
        
            byte[] digest = CryptoHelper.SHAHash( bb.ToArray() );

            return BitConverter.ToUInt64( digest, 0 );
        }


        static uint ToUnixTime( DateTime dt )
        {
            TimeSpan ts = ( dt - new DateTime( 1970, 1, 1, 0, 0, 0 ) );
            return ( uint )ts.TotalSeconds;
        }


        static ulong GetMicroseconds()
        {
            return 0xDCBFFEFF2BC000UL + ( ( ulong )ToUnixTime( DateTime.UtcNow ) * 1000000UL );
        }

        static byte[] GetPlaintext( ulong timeStamp, uint internalIp )
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( timeStamp );
            bb.Append( internalIp );

            return bb.ToArray();
        }

        static void Main( string[] args )
        {
            byte[] intIpData = Dns.GetHostByName( Dns.GetHostName() ).AddressList[ 0 ].GetAddressBytes();
            uint internalIp = BitConverter.ToUInt32( intIpData, 0 );

            string userName = ""; // fill this in
            string password = ""; // fill this in


            ServerClient client = new ServerClient();

            client.ConnectToServer( new IPEndPoint( IPAddress.Parse( "72.165.61.139" ), 27039 ) );


            // send protocol and ip request packet
            ByteBuffer bb = new ByteBuffer( true );

            bb.Append( ( uint )0 );
            bb.Append( ( byte )4 );
            bb.Append( internalIp );
            bb.Append( ( uint )0 );

            client.socket.Writer.Write( bb.ToArray() );


            byte result = client.socket.Reader.ReadByte();
            uint externalIp = ( uint )IPAddress.NetworkToHostOrder( client.socket.Reader.ReadInt32() );

            client.SendCommand( 2, ( ushort )userName.Length, userName, ( ushort )userName.Length, userName );

            uint salt1 = client.socket.Reader.ReadUInt32();
            uint salt2 = client.socket.Reader.ReadUInt32();

            byte[] aesKey = GenerateAESKey( salt1, salt2, password );
            byte[] iv = CryptoHelper.GenerateRandomBlock( 16 );

            // time data
            ulong microseconds = GetMicroseconds() ^ GetObfuscationMask( internalIp, externalIp );

            byte[] plainText = GetPlaintext( microseconds, internalIp );
            byte[] cipherText = CryptoHelper.AESEncrypt( plainText, aesKey, iv );

            // reply data
            TcpPacket packet = new TcpPacket();
            packet.Append( iv );
            packet.Append( ( ushort )plainText.Length );
            packet.Append( ( ushort )cipherText.Length );
            packet.Append( cipherText );

            client.socket.Send( packet );
 
            result = client.socket.Reader.ReadByte();
            ulong loginTime = client.socket.Reader.ReadUInt64();
            ulong unk = client.socket.Reader.ReadUInt64();

            packet = client.RecvPacket();

            DataStream ds = new DataStream( packet.GetPayload(), true );

            // client tgt
            ushort versionNum = ds.ReadUInt16();
            byte[] tgtIv = ds.ReadBytes( 16 );

            ushort tgtPlaintextSize = ds.ReadUInt16();
            ushort tgtCiphertextSize = ds.ReadUInt16();

            byte[] tgtEncrypted = ds.ReadBytes( tgtCiphertextSize );
            byte[] tgtPlaintext = CryptoHelper.AESDecrypt( tgtEncrypted, aesKey, tgtIv );

            ClientTGT tgt = ClientTGT.Deserialize( tgtPlaintext );

            // server tgt
            ushort serverTGTSize = ds.ReadUInt16();
            byte[] serverTGT = ds.ReadBytes( serverTGTSize );

            File.WriteAllBytes( "tgt_server.bin", serverTGT );

            // account record: encrypted CMultiFieldBlob
            uint accRecordSize = ds.ReadUInt32();
            byte[] accRecord = ds.ReadBytes( ds.SizeRemaining() );

            File.WriteAllBytes( "acc_record.bin", accRecord );


#if false
            GDSClient gdsClient = new GDSClient();
            CSDSClient csdsClient = new CSDSClient();

            // ensure latest GDS list
            foreach ( string gdsServer in GDSClient.GDServers )
            {
                ServerCache.AddServer( EServerType.GeneralDirectoryServer, GetAddress( gdsServer ) );

                IPEndPoint[] gdsList = gdsClient.GetServerList( GetAddress( gdsServer ), EServerType.GeneralDirectoryServer, null );
                ServerCache.AddServers( EServerType.GeneralDirectoryServer, gdsList );
            }

            foreach ( IPEndPoint gdsServer in ServerCache.GetServers( EServerType.GeneralDirectoryServer ).GetServers() )
            {
                for ( EServerType serverType = 0 ; ( int )serverType < 30 ; ++serverType )
                {
                    Console.Write( "Asking GDS {0} for {1}... ", gdsServer, serverType );

                    try
                    {
                        IPEndPoint[] serverList = gdsClient.GetServerList( gdsServer, serverType, null );

                        if ( serverList == null )
                            serverList = gdsClient.GetServerList( gdsServer, serverType, "username" );

                        if ( serverList == null )
                        {
                            Console.WriteLine( "Failed!" );
                            continue;
                        }

                        Console.WriteLine( "Got {0} servers.", serverList.Length );

                        ServerCache.AddServers( serverType, serverList );
                    }
                    catch
                    {
                        Console.WriteLine( "Failed!" );
                        continue;
                    }

                }
            }

            foreach ( IPEndPoint csdsServer in ServerCache.GetServers( EServerType.CSDS ).GetServers() )
            {
                for ( EServerType serverType = 0 ; ( int )serverType < 30 ; ++serverType )
                {
                    Console.Write( "Asking CSDS {0} for {1}... ", csdsServer, serverType );
                    try
                    {
                        IPEndPoint[] serverList = csdsClient.GetServerList( csdsServer, serverType, null );

                        if ( serverList == null )
                            serverList = csdsClient.GetServerList( csdsServer, serverType, "username" );

                        if ( serverList == null )
                        {
                            Console.WriteLine( "Failed!" );
                            continue;
                        }

                        Console.WriteLine( "Got {0} servers.", serverList.Length );

                        ServerCache.AddServers( serverType, serverList );
                    }
                    catch
                    {
                        Console.WriteLine( "Failed!" );
                        continue;
                    }
                }
            }


            foreach ( ServerList serverList in ServerCache.GetServerLists() )
            {
                IPEndPoint[] servers = serverList.GetServers();

                File.AppendAllText( "servers.txt", string.Format( "Server type: {0} ({1} servers)\r\n", serverList.Type, servers.Length ) );

                foreach ( IPEndPoint server in servers )
                {
                    File.AppendAllText( "servers.txt", string.Format( "\t{0}\r\n", server ) );

                    DSClient dsClient = new DSClient();

                    for ( EServerType serverType = 0 ; ( int )serverType < 30 ; ++serverType )
                    {
                        try
                        {
                            IPEndPoint[] innerServers = dsClient.GetServerList( server, serverType, null );

                            if ( innerServers != null )
                            {
                                File.AppendAllText( "servers.txt", "\t\tMay be a DS server!\r\n" );
                                break;
                            }
                        }
                        catch { }
                    }
                }

                File.AppendAllText( "servers.txt", "\r\n" );
            }


            //CMInterface cmInterface = new CMInterface();

            //cmInterface.ConnectToCM();

            
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
            }

            
            IPEndPoint[] csdsList = gdsClient.GetServerList( GetAddress( GDSClient.GDServers[ 0 ] ), EServerType.CSDS, null );

            List<IPEndPoint> fullCSList = new List<IPEndPoint>();

            CSDSClient csdsClient = new CSDSClient();

            foreach ( IPEndPoint csEndPoint in csdsList )
            {
            }
#endif

        }


        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        class ClientTGT : Serializable<ClientTGT>
        {
            [MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 )]
            public byte[] AccountRecordKey;

            public ushort Unknown;

            public ulong SteamGlobalUserID;

            public IPAddrPort Server1;
            public IPAddrPort Server2;

            public MicroTime CreationTime;
            public MicroTime ExpirationTime;
        }
    }
}