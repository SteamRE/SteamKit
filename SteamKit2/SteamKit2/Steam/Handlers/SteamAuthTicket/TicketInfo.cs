using System;
using System.IO.Hashing;

namespace SteamKit2
{
    public sealed partial class SteamAuthTicket
    {
        /// <summary>
        /// Represents a valid authorized session ticket.
        /// </summary>
        public class TicketInfo : IDisposable
        {
            /// <summary>
            /// Application the ticket was generated for.
            /// </summary>
            internal uint AppID { get; }
            /// <summary>
            /// Bytes of the valid Session Ticket
            /// </summary>
            public byte[] Ticket { get; }
            internal uint TicketCRC { get; }

            internal TicketInfo( SteamAuthTicket handler, uint appID, byte[] ticket )
            {
                _handler = handler;
                AppID = appID;
                Ticket = ticket;
                TicketCRC = Crc32.HashToUInt32( ticket );
            }

            /// <summary>
            /// Discards the ticket.
            /// </summary>
            public void Dispose()
            {
                _handler.CancelAuthTicket( this );
                System.GC.SuppressFinalize( this );
            }

            private readonly SteamAuthTicket _handler;
        }
    }
}
