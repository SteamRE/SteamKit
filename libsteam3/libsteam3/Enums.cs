using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibSteam3
{
    public class Steam3Enums
    {
        public enum PktType :byte
        {
            // This is the first packet type sent to Steam servers by the client.
            // The client iterates through approximately 20 servers in an attempt to find the "best" one.
            // Only the UDPPktType, local connection ID and outgoing sequence need to be given an appropriate value to request a challenge.
            ChallengeReq = 1,

            // Steam servers respond to k_EUDPPktTypeChallengeReq with this packet type value and 8 bytes of information.
            // The data is not encrypted.
            // The first uint32 is the non-obfuscated challenge. See the login sequence page for obfuscation information.
            // The next uint32 is unconfirmed but suspected to be ping.
            Challenge = 2,

            // The client sends this packet type after choosing the "best" Steam server available.
            // The obfuscated challenge is attached.
            // The Steam client uses the flag 4 when sending this, so assume it is necessary.
            // UDPPktHdr should be filled in as normal but without encryption on the data.
            Connect = 3,

            // If the k_EUDPPktTypeConnect packet is received by the destination server and acknowledged as valid then it responds with this packet type.
            // No data is attached, however a destination connection ID is generated and should be stored for use in later traffic.
            // The Steam client uses the flag 4 when sending this, so assume it is necessary.
            Accept = 4,

            // Unknown, most likely sent to signify process termination.
            Disconnect = 5,

            // This packet type is used for the majority of VS01 traffic, incoming and outgoing.
            // The flag is usually 4, however this is not confirmed as necessary or constant.
            // The packet should include valid destination and source connection IDs.
            // Not all data sent through this type is encrypted and it's currently unclear what indicates when it is and when it isn't.
            Data = 6,

            // The datagram message type appears to be used for a packet resend.
            // Sometimes the sequence number isn't included, but message size is.
            // Sometimes the sequence number is included, but message size isn't.
            // Sequence number may be replaced by size in the case of the seq value being incremented between initial send time and retry time.
            Datagram = 7,

            // Max enum value.
            Max = 8,
        };

        public struct NetFlags
        {
            public const uint NoIOCP = 1;
            public const uint FindAvailPort = 2;
            public const uint UseAuthentication = 4;
            public const uint UseEncryption = 8;
            public const uint RawStream = 16;
            public const uint RawStreamSend = 32;
            public const uint UnboundSocket = 64;
            public const uint RawIORecv = 128;
        }
    }
}
