using System;
using System.IO.Hashing;

namespace SteamKit2
{
    /// <summary>
    /// Represents a valid authorized session ticket.
    /// </summary>
    public sealed partial class TicketInfo : IDisposable
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
            TicketCRC = BitConverter.ToUInt32( Crc32.Hash( ticket ), 0 );
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
