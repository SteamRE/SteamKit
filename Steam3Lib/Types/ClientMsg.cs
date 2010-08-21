using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamLib
{
    public interface IMsg
    {
        void SetEMsg( EMsg eMsg );
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgHdr : Serializable<MsgHdr>, IMsg
    {
        public EMsg EMsg;

        public ulong TargetJobID;
        public ulong SourceJobID;


        public MsgHdr()
        {
            this.EMsg = EMsg.Invalid;

            this.TargetJobID = UInt64.MaxValue;
            this.SourceJobID = UInt64.MaxValue;
        }

        public void SetEMsg( EMsg eMsg )
        {
            this.EMsg = eMsg;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class ExtendedClientMsgHdr : Serializable<ExtendedClientMsgHdr>, IMsg
    {
        public EMsg EMsg;

        public byte HeaderSize;

        public ushort HeaderVersion;

        public ulong TargetJobID;
        public ulong SourceJobID;

        public byte HeaderCanary;

        public ulong SteamID;

        public int SessionID;


        public ExtendedClientMsgHdr()
        {
            this.EMsg = EMsg.Invalid;

            this.HeaderSize = 36;

            this.HeaderVersion = 2;

            this.TargetJobID = UInt64.MaxValue;
            this.SourceJobID = UInt64.MaxValue;

            this.HeaderCanary = 239;

            this.SessionID = 0;
        }

        public void SetEMsg( EMsg eMsg )
        {
            this.EMsg = eMsg;
        }

    }


    public class ClientMsg<MsgHdr, Hdr>
        : Serializable<Hdr>
        where Hdr : Serializable<Hdr>, IMsg, new()
        where MsgHdr : Serializable<MsgHdr>, IClientMsg, new()
    {

        public Hdr Header { get; private set; }
        public MsgHdr MsgHeader { get; private set; }

        ByteBuffer byteBuff;


        public ClientMsg()
        {
            byteBuff = new ByteBuffer();

            Header = new Hdr();
            MsgHeader = new MsgHdr();

            Header.SetEMsg( MsgHeader.GetEMsg() );
        }

        public ClientMsg( byte[] data )
        {
            byteBuff = new ByteBuffer();

            int headerSize = Marshal.SizeOf( typeof( Hdr ) );

            this.Header = Serializable<Hdr>.Deserialize( data );
            this.MsgHeader = Serializable<MsgHdr>.Deserialize( data, headerSize );

            headerSize += Marshal.SizeOf( typeof( MsgHdr ) );

            byte[] payload = new byte[ data.Length - headerSize ];
            Array.Copy( data, headerSize, payload, 0, payload.Length );

            SetPayload( payload );
        }


        public void SetPayload( byte[] data )
        {
            byteBuff.Clear();
            byteBuff.Append( data );
        }

        public byte[] GetPayload()
        {
            return byteBuff.ToArray();
        }


        public byte[] GetData()
        {
            ByteBuffer bb = new ByteBuffer();

            bb.Append( this.Header.Serialize() );
            bb.Append( this.MsgHeader.Serialize() );

            bb.Append( byteBuff.ToArray() );

            return bb.ToArray();
        }


        public void Write<T>( T obj ) where T : struct
        {
            byteBuff.Append( obj );
        }
        public void Write( byte[] obj )
        {
            byteBuff.Append( obj );
        }


        public static Hdr GetHeader( byte[] data )
        {
            return new ClientMsg<MsgHdr, Hdr>( data ).Header;
        }
        public static MsgHdr GetMsgHeader( byte[] data )
        {
            return new ClientMsg<MsgHdr, Hdr>( data ).MsgHeader;
        }
        public static byte[] GetPayload( byte[] data )
        {
            return new ClientMsg<MsgHdr, Hdr>( data ).GetPayload();
        }
    }

    public interface IClientMsg
    {
        EMsg GetEMsg();
    }


    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgChannelEncryptRequest : Serializable<MsgChannelEncryptRequest>, IClientMsg
    {
        public uint ProtocolVersion;
        public EUniverse Universe;

        public MsgChannelEncryptRequest()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ChannelEncryptRequest;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgChannelEncryptResponse : Serializable<MsgChannelEncryptResponse>, IClientMsg
    {
        public const uint PROTOCOL_VERSION = 1;

        public uint ProtocolVersion;
        public uint KeySize;

        public MsgChannelEncryptResponse()
        {
            ProtocolVersion = PROTOCOL_VERSION;
            KeySize = 128;
        }

        public EMsg GetEMsg()
        {
            return EMsg.ChannelEncryptResponse;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgChannelEncryptResult : Serializable<MsgChannelEncryptResult>, IClientMsg
    {
        public EResult Result;

        public MsgChannelEncryptResult()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ChannelEncryptResult;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientAnonLogOn : Serializable<MsgClientAnonLogOn>, IClientMsg
    {
        public const uint ObfuscationMask = 0xBAADF00D;
        public const uint CurrentProtocol = 65563;

        public uint ProtocolVersion;

        public uint PrivateIPObfuscated;
        public uint PublicIP;

        public MsgClientAnonLogOn()
        {
            ProtocolVersion = CurrentProtocol;
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientAnonLogOn;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientLogOnResponse : Serializable<MsgClientLogOnResponse>, IClientMsg
    {
        public EResult Result;

        public int OutOfGameHeartbeatRateSec;
        public int InGameHeartbeatRateSec;

        public ulong ClientSuppliedSteamId;

        public uint IPPublic;

        public uint ServerRealTime;

        public MsgClientLogOnResponse()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientLogOnResponse;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgMulti : Serializable<MsgMulti>, IClientMsg
    {
        public uint UnzippedSize;

        public MsgMulti()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.Multi;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientCMList : Serializable<MsgClientCMList>, IClientMsg
    {
        public int CountCMs;

        public MsgClientCMList()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientCMList;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientServersAvailable : Serializable<MsgClientServersAvailable>, IClientMsg
    {
        public int m_Unk1;
        public int m_Unk2;
        public int m_Unk3;
        public int m_Unk4;
        public int m_Unk5;
        public int m_cCM;

        public MsgClientServersAvailable()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientServersAvailable;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientServerList : Serializable<MsgClientServerList>, IClientMsg
    {
        public int ServerCount;

        public MsgClientServerList()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientServerList;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientRequestedClientStats : Serializable<MsgClientRequestedClientStats>, IClientMsg
    {
        public int StatCount;

        public MsgClientRequestedClientStats()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientRequestedClientStats;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientHeartBeat : Serializable<MsgClientHeartBeat>, IClientMsg
    {
        public MsgClientHeartBeat()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientHeartBeat;
        }
    }
}
