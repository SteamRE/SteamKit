package net.steam3.packets.msg;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.types.EResult;
import steamkit.util.ISerializable;

public class ChannelEncryptResult implements ISerializable
{
	private static final int Size = 4;
	
	private EResult result;
	
	public ChannelEncryptResult()
	{
		result = EResult.Invalid;
	}
	
	public EResult getResult()
	{
		return result;
	}
	
	public EMsg getMsg()
	{
		return EMsg.ChannelEncryptResult;
	}
	
	public static ChannelEncryptResult deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static ChannelEncryptResult deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ChannelEncryptResult msg = new ChannelEncryptResult();
		
		msg.result = EResult.lookupResult( buf.getInt() );
		
		return msg;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( result.getCode() );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
