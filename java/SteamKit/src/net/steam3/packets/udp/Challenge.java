package net.steam3.packets.udp;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class Challenge implements ISerializable
{
	public static final int CHALLENGE_MASK = 0xA426DF2B;
	private static final int Size = 8;
	
	private int challenge;
	private int load;
	
	public Challenge()
	{
		this.challenge = 0;
		this.load = 0;
	}
	
	public int getChallenge()
	{
		return challenge;
	}
	
	public int getLoad()
	{
		return load;
	}
	
	public static Challenge deserialize( ChannelBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		Challenge challenge = new Challenge();
		
		challenge.challenge = buf.readInt();
		challenge.load = buf.readInt();
		
		return challenge;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( challenge );
		serialBuffer.putInt( load );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
