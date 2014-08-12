using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaBot
{
    public enum Events
    {
        Connected,
        Disconnected,
        DotaGCReady,
        DotaToMainMenu,
        DotaJoinedLobby,
        DotaLeftLobby,
        DotaCreatedLobby,
        DotaFailedLobby,
        DotaFoundLobby,
        DotaGCDisconnect,
        LogonFailSteamGuard,
        LogonFailBadCreds,
        AttemptReconnect,
    }
}
