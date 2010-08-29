package net.steam3.packets.msg;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class MultiMsg implements ISerializable
{
	private static final int Size = 4;
	
	private int bufferLength;
	
	public MultiMsg()
	{
		bufferLength = 0;
	}
	
	public int getLength()
	{
		return bufferLength;
	}
	
	public EMsg getMsg()
	{
		return EMsg.Multi;
	}
	
	public static MultiMsg deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static MultiMsg deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		MultiMsg msg = new MultiMsg();
		
		msg.bufferLength = buf.getInt();
		
		return msg;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( bufferLength );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
