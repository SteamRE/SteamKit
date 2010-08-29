package net.steam3;

import java.net.InetSocketAddress;

import net.steam3.packets.udp.UDPPacket;

public interface IUDPHandlerCallback
{
	public void UDPRecvCallback( UDPPacket packet, Object message, InetSocketAddress remoteEndpoint );
}
