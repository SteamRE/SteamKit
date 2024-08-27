using System.Buffers;
using System.IO;
using System.Security.Cryptography;

namespace SteamKit2
{
    /// <summary>
    /// Writes random non zero bytes into user data space of the stream.
    /// </summary>
    public class RandomUserDataSerializer : IUserDataSerializer
    {
        static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

        /// <inheritdoc/>
        public void Serialize( BinaryWriter writer )
        {
            var bytes = ArrayPool<byte>.Shared.Rent( 8 );
            try
            {
                _random.GetNonZeroBytes( bytes );
                writer.Write( bytes, 0, 8 );
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( bytes );
            }
        }
    }
}
