using System.Reflection;
using SteamKit2;

namespace Tests;

public static class ConnectedSteamClient
{
#if DEBUG
    public static SteamClient Get()
    {
        var client = new SteamClient();
        client.SetIsConnected( true );
        
        return client;
    }
#endif
}
