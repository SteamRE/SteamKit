using System.Reflection;
using SteamKit2;

namespace Tests;

public static class ConnectedSteamClient
{
    public static SteamClient Get()
    {
        var client = new SteamClient();
        PropertyInfo property = typeof(SteamClient).GetProperty(nameof(client.IsConnected));
        property = property.DeclaringType.GetProperty(nameof(client.IsConnected));
        property.GetSetMethod(true).Invoke(client, new object[] { true });

        return client;
    }
}
