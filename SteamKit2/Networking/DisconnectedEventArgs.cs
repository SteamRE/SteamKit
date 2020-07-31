/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;

namespace SteamKit2
{
    class DisconnectedEventArgs : EventArgs
    {
        public bool UserInitiated { get; }

        public DisconnectedEventArgs( bool userInitiated )
        {
            this.UserInitiated = userInitiated;
        }
    }

}
