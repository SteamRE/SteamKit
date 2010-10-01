package net.steam3;

import java.net.InetSocketAddress;

public interface IUDPHandlerCallback
{
	public void UDPRecvCallback( UDPPacket packet, Object message, InetSocketAddress remoteEndpoint );
}
