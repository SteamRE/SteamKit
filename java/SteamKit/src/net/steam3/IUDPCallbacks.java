package net.steam3;

import java.net.InetSocketAddress;

import steamkit.steam3.SteamLanguage.ChallengeData;
import steamkit.steam3.SteamLanguage.*;

public interface IUDPCallbacks {
	
	public void ChalllengeResponseCallback( ChallengeData challenge, InetSocketAddress remoteEndpoint );
	public void AcceptCallback( Accept accept );
	public void DisconnectCallback( Disconnect disconnect );
	public void RecvDataCallback( Data data, InetSocketAddress remoteEndpoint );
	
	public void SessionNegotiationInitiated( EUniverse universe );
	public void SessionNegotiationCompleted( EResult result );
}
