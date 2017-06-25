/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



namespace SteamKit2
{
    interface INetFilterEncryption
    {
        byte[] ProcessIncoming( byte[] data );
        byte[] ProcessOutgoing( byte[] data );
    }
}
