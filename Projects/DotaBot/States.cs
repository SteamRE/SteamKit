using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaBot
{
    public enum States
    {
        Connecting,
        Disconnected,
        Connected,
        DisconnectNoRetry,
        DisconnectRetry,
        #region DOTA
        Dota,
        DotaConnect,
        DotaMenu,
        #region DOTAJOIN
        DotaJoinLobby,
        DotaJoinFind,
        DotaJoinEnter,
        #endregion
        DotaLobby
        #endregion
    }
}
