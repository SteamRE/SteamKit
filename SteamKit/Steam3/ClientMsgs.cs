using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SteamKit
{   
    interface IClientMsg
    {
        // so that IMsgHdr's can get set
        EMsg GetEMsg();
    }


    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgChannelEncryptRequest : Serializable<MsgChannelEncryptRequest>, IClientMsg
    {
        public const uint PROTOCOL_VERSION = 1;

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
        public const uint CurrentProtocol = 65563; // ??

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

        public ulong ClientSuppliedSteamID;

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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MsgClientChangeStatus : Serializable<MsgClientChangeStatus>, IClientMsg
    {
        public byte personaState;

        public MsgClientChangeStatus()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientChangeStatus;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientFriendMsg : Serializable<MsgClientFriendMsg>, IClientMsg
    {
        public ulong SteamID;
        public EChatEntryType EntryType;

        public MsgClientFriendMsg()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientFriendMsg;
        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 1 )]
    class MsgClientFriendMsgIncoming : Serializable<MsgClientFriendMsgIncoming>, IClientMsg
    {
        public ulong SteamID;
        public EChatEntryType EntryType;
        public int MessageSize;

        public MsgClientFriendMsgIncoming()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientFriendMsgIncoming;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MsgGSServerType : Serializable<MsgGSServerType>, IClientMsg
    {
        public uint appIdServed;
        public uint flags;
        public uint gameIP;
        public short gamePort;

        public MsgGSServerType()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.GSServerType;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MsgClientNewLoginKey : Serializable<MsgClientNewLoginKey>, IClientMsg
    {
        public uint uniqueID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] loginKey;

        public MsgClientNewLoginKey()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientNewLoginKey;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MsgClientNewLoginKeyAccepted : Serializable<MsgClientNewLoginKeyAccepted>, IClientMsg
    {
        public uint uniqueID;

        public MsgClientNewLoginKeyAccepted()
        {
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientNewLoginKeyAccepted;
        }
    }

    enum ENetQOSLevel : uint
    {
        NotSet = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    };

    [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1 )]
    class MsgClientLogOnWithCredentials : Serializable<MsgClientLogOnWithCredentials>, IClientMsg
    {
        public const uint ObfuscationMask = 0xBAADF00D;
        public const uint CurrentProtocol = 65563; // ??

        public uint ProtocolVersion;

        public uint PrivateIPObfuscated;
        public uint PublicIP;

        public ulong ClientSuppliedSteamID;

        public uint TicketLength;

        [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 64 )]
        public string AccountName;
        [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 20 )]
        public string Password;

        public ENetQOSLevel QOSLevel;

        public MsgClientLogOnWithCredentials()
        {
            ProtocolVersion = CurrentProtocol;

            AccountName = "";
            Password = "";
        }

        public EMsg GetEMsg()
        {
            return EMsg.ClientLogOnWithCredentials;
        }
    }
}
