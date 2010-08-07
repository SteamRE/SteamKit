using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace LibSteam3
{
    /// <summary>
    /// Steam3 connection and protocol handler
    /// </summary>
    public class Steam3Socket
    {
        UdpClient Socket;
        Boolean Connected, Encrypted, Authenticate;
        Dictionary<uint, FragmentedMsg> FragMsgs = new Dictionary<uint,FragmentedMsg>();
        UInt32 ClientSeqAck = 0, ClientSeqCurrent = 1, ServerConnID = 0;

        /// <summary>
        /// Create a new connection and handler to Steam3
        /// </summary>
        /// <param name="Dest">Hostname of destination</param>
        /// <param name="Port">Port of destination</param>
        public Steam3Socket(String Dest, int Port)
        {
            Console.WriteLine("LibSteam3 init...");
            Socket = new UdpClient(Dest, Port);
            Encrypted = false; Authenticate = false; Connected = true;
            new Thread(Reciever).Start();
            ProcessAndSend(new byte[0], Steam3Enums.PktType.ChallengeReq);
        }

        void Reciever()
        {
            Console.WriteLine("Started recieve thread.");
            IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0); // idk what i'm doing
            BinaryReader Reader;

            while (Connected)
            {
                byte[] Buffer = Socket.Receive(ref EndPoint);
                Reader = new BinaryReader(new MemoryStream(Buffer));

                if (new string(Reader.ReadChars(4)) == "VS01")
                {
                    ushort PktLen = Reader.ReadUInt16();
                    Steam3Enums.PktType PktType = (Steam3Enums.PktType)Reader.ReadByte();
                    byte PktFlags = Reader.ReadByte();

                    uint SrcConnID = Reader.ReadUInt32();
                    ServerConnID = SrcConnID;
                    uint DestConnID = Reader.ReadUInt32();

                    uint SeqCurrent = Reader.ReadUInt32();
                    ClientSeqAck = SeqCurrent;
                    uint SeqAck = Reader.ReadUInt32();
                    ClientSeqCurrent = ++SeqAck;

                    uint PktsInMsg = Reader.ReadUInt32();
                    uint MsgStartSeq = Reader.ReadUInt32();

                    uint MsgLen = Reader.ReadUInt32();
                    byte[] PktData = Reader.ReadBytes(PktLen);

                    Console.WriteLine("<- incoming {0}, {1} bytes", PktType, Buffer.Length);

                    switch (PktType)
                    {
                        case Steam3Enums.PktType.Challenge:
                            Authenticate = true;
                            uint Challenge = BitConverter.ToUInt32(PktData, 0);
                            byte[] MaskedChl = BitConverter.GetBytes(Challenge ^ 0xA426DF2B);
                            Console.WriteLine("   got challenge 0x{0:x}, sending 0x{1:x}...", Challenge, BitConverter.ToInt32(MaskedChl, 0));
                            ProcessAndSend(MaskedChl, Steam3Enums.PktType.Connect);
                            break;
                    }

                    // future note: this is for normal packets or something!!
                    if (PktsInMsg > 1)
                    {
                        //BuildFragmentedMessage(MsgStartSeq, PktsInMsg, SeqCurrent, PktData);
                    }
                    else
                    {
                        //ProcessMessage(PktData);
                    }
                }
            }
        }

        /// <summary>
        /// Processes message and sends to Steam server.
        /// </summary>
        /// <param name="Data">Message to send</param>
        public void ProcessAndSend(byte[] Data)
        {
            // TODO: fragmentation (seperate method?)
            ProcessAndSend(Data, Steam3Enums.PktType.Data);
        }

        /// <summary>
        /// Processes message and sends to Steam server.
        /// </summary>
        /// <param name="Data">Message to send</param>
        /// <param name="PacketType">Type of message to send</param>
        public void ProcessAndSend(byte[] Data, Steam3Enums.PktType PacketType)
        {
            if (Connected)
            {
                if (Encrypted)
                {
                    //EncryptMessage(Data);
                }

                MemoryStream Stream = new MemoryStream();
                BinaryWriter Writer = new BinaryWriter(Stream);

                Writer.Write("VS01".ToCharArray()); // magic
                Writer.Write((ushort)Data.Length); // packet len

                Writer.Write((byte)PacketType); // packet type
                Writer.Write((byte)((Authenticate) ? Steam3Enums.NetFlags.UseAuthentication : 0)); // packet flags

                Writer.Write(0x200); // src connection id
                Writer.Write(ServerConnID); // dest connection id

                Writer.Write(ClientSeqCurrent); // seq this
                Writer.Write(ClientSeqAck); // seq acked
                
                Writer.Write(1); // num of packets for this message
                Writer.Write(ClientSeqCurrent); // start seq for this message

                Writer.Write(Data.Length); // message len
                Writer.Write(Data);

                byte[] Final = Stream.ToArray();

                Socket.Send(Final, Final.Length);
            }
        }

        void BuildFragmentedMessage(uint MsgSeq, uint NumFrags, uint FragSeq, byte[] FragData)
        {
            if (!FragMsgs.ContainsKey(MsgSeq))
                FragMsgs.Add(MsgSeq, new FragmentedMsg(NumFrags));

            FragMsgs[MsgSeq].AddFragment(FragSeq, FragData);

            if (FragMsgs[MsgSeq].Completed)
                //ProcessMessage(FragMsgs[MsgSeq]);
                FragMsgs.Remove(MsgSeq);
        }

        class FragmentedMsg
        {
            public Boolean Completed;
            public Dictionary<uint, byte[]> Fragments = new Dictionary<uint,byte[]>();
            public uint NumFragments;

            public FragmentedMsg(uint NumFrags)
            {
                NumFragments = NumFrags;
            }

            public void AddFragment(uint Seq, byte[] Data)
            {
                Fragments.Add(Seq, Data);

                if (Fragments.Count == NumFragments)
                    Completed = true;
            }
        }
    }
}