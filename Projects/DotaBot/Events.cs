// File Name:      Events.cs
// Project:           DotaBot
// Copyright (c) christian stewart 2014
// 
// All rights reserved.

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
		DotaStartFindLobby,
        DotaCreatedLobby,
        DotaFailedLobby,
        DotaFoundLobby,
        DotaGCDisconnect,
        LogonFailSteamGuard,
        LogonFailBadCreds,
        AttemptReconnect,
    }
}