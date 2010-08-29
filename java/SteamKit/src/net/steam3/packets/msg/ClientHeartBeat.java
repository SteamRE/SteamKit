package net.steam3.packets.msg;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class ClientHeartBeat implements ISerializable
{
	private static final int Size = 0;
	
	public ClientHeartBeat()
	{
	}
	
	public EMsg getMsg()
	{
		return EMsg.ClientHeartBeat;
	}
	
	public static ClientHeartBeat deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static ClientHeartBeat deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ClientHeartBeat msg = new ClientHeartBeat();
		
		return msg;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
