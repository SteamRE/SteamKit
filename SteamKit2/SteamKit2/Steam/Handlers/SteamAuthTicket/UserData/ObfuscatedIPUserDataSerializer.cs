using System.IO;

namespace SteamKit2
{
    internal class ObfuscatedIPUserDataSerializer( SteamAuthTicket steamAuthTicket ) : IUserDataSerializer
    {
        public void Serialize( BinaryWriter writer )
        {
            static uint ObfuscateIP( uint value )
            {
                var temp = 0x85EBCA6B * ( value ^ ( value >> 16 ) );
                var outvalue = 0xC2B2AE35 * ( temp ^ ( temp >> 13 ) );
                return outvalue ^ ( outvalue >> 16 );
            }

            writer.Write( ObfuscateIP( _steamAuthTicket.PublicIP ) );
            writer.Write( ObfuscateIP( _steamAuthTicket.LocalIP ) );
        }

        private readonly SteamAuthTicket _steamAuthTicket = steamAuthTicket;
    }
}
