using System.Diagnostics;
using System.IO;

namespace SteamKit2
{
    /// <summary>
    /// Handles serialization of user specific data in auth token generation process
    /// </summary>
    public interface IUserDataSerializer
    {
        /// <summary>
        /// Writes user data into provided buffer.
        /// </summary>
        /// <param name="writer">Buffer to write data into.</param>
        void Serialize( BinaryWriter writer );
    }

    /// <summary>
    /// Handles building auth tickets.
    /// </summary>
    public interface IAuthTicketBuilder
    {
        /// <summary>
        /// Builds auth ticket with specified <paramref name="gameConnectToken"/>.
        /// </summary>
        /// <param name="gameConnectToken">Valid GameConnect token.</param>
        /// <returns>Bytes of auth ticket to send to steam servers.</returns>
        byte[] Build( byte[] gameConnectToken );
    }

    /// <summary>
    /// Handles generation of auth ticket.
    /// </summary>
    /// <remarks>
    /// Creates instance of auth ticket builder, with specified <paramref name="userDataSerializer"/> being used.
    /// </remarks>
    /// <param name="userDataSerializer"></param>
    public class DefaultAuthTicketBuilder( IUserDataSerializer userDataSerializer ) : IAuthTicketBuilder
    {
        /// <inheritdoc/>
        public byte[] Build( byte[] gameConnectToken )
        {

            const int sessionSize =
                4 + // unknown, always 1
                4 + // unknown, always 2
                4 + // public IP v4, optional
                4 + // private IP v4, optional
                4 + // timestamp & uint.MaxValue
                4;  // sequence

            using var stream = new MemoryStream( gameConnectToken.Length + 4 + sessionSize );
            using ( var writer = new BinaryWriter( stream ) )
            {
                writer.Write( gameConnectToken.Length );
                writer.Write( gameConnectToken );

                writer.Write( sessionSize );
                writer.Write( 1 );
                writer.Write( 2 );

                _userDataSerializer.Serialize( writer );
                writer.Write( ( uint )Stopwatch.GetTimestamp() );
                writer.Write( ++_sequence );
            }
            return stream.ToArray();
        }

        private uint _sequence;
        private readonly IUserDataSerializer _userDataSerializer = userDataSerializer;
    }
}
