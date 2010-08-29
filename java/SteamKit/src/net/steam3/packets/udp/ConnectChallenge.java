package net.steam3.packets.udp;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class ConnectChallenge implements ISerializable
{
	public static final int CHALLENGE_MASK = 0xA426DF2B;
	private static final int Size = 4;
	
	private int challenge;
	
	public ConnectChallenge()
	{
		this.challenge = 0;
	}
	
	public void setChallenge( int challenge )
	{
		this.challenge = challenge;
	}
	
	public static ConnectChallenge deserialize( ChannelBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ConnectChallenge challenge = new ConnectChallenge();
		
		challenge.challenge = buf.readInt();
		
		return challenge;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( challenge );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
