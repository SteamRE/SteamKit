package net.steam3;

import java.net.InetSocketAddress;

import steamkit.types.EResult;
import steamkit.types.EUniverse;

import net.steam3.packets.udp.*;

public interface IUDPCallbacks {
	
	public void ChalllengeResponseCallback( Challenge challenge, InetSocketAddress remoteEndpoint );
	public void AcceptCallback( Accept accept );
	public void DisconnectCallback( Disconnect disconnect );
	public void RecvDataCallback( Data data, InetSocketAddress remoteEndpoint );
	
	public void SessionNegotiationInitiated( EUniverse universe );
	public void SessionNegotiationCompleted( EResult result );
}
