using System;
using System.Collections.Generic;
using System.Text;

namespace SteamKit2
{
    /// <summary>
    /// Represents a valid authorized session ticket.
    /// </summary>
    public class TicketInfo : IDisposable
    {
        internal uint AppID { get; }
        internal uint CRC { get; }
        /// <summary>
        /// Bytes of the valid Session Ticket
        /// </summary>
        public byte[] Ticket { get; }

        internal TicketInfo( SteamAuthTicket handler, uint appid, uint crc, byte[] ticket )
        {
            _handler = handler;
            AppID = appid;
            CRC = crc;
            Ticket = ticket;
        }

        /// <summary>
        /// Tell steam we no longer use the ticket.
        /// </summary>
        public void Dispose()
        {
            _handler.CancelAuthTicket( this );
        }

        private readonly SteamAuthTicket _handler;
    }
}
