/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */


using System;
using SteamKit2.Discovery;

namespace SteamKit2
{
    struct SteamConfigurationState
    {
        public bool AllowDirectoryFetch;
        public uint CellID;
        public TimeSpan ConnectionTimeout;
        public EClientPersonaStateFlag DefaultPersonaStateFlags;
        public HttpClientFactory HttpClientFactory;
        public ProtocolTypes ProtocolTypes;
        public IServerListProvider ServerListProvider;
        public EUniverse Universe;
        public Uri WebAPIBaseAddress;
        public string WebAPIKey;
    }
}
