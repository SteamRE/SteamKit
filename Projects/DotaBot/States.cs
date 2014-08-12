// File Name:      States.cs
// Project:           DotaBot
// Copyright (c) christian stewart 2014
// 
// All rights reserved.

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