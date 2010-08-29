package net.steam3.packets.msg;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.types.EUniverse;
import steamkit.util.ISerializable;

public class ChannelEncryptRequest implements ISerializable
{
	private static final int Size = 8;
	
	private int protocolVersion;
	private EUniverse universe;
	
	public ChannelEncryptRequest()
	{
		protocolVersion = 0;
		universe = EUniverse.Invalid;
	}
	
	public EUniverse getUniverse()
	{
		return universe;
	}
	
	public EMsg getMsg()
	{
		return EMsg.ChannelEncryptRequest;
	}
	
	public static ChannelEncryptRequest deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static ChannelEncryptRequest deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ChannelEncryptRequest msg = new ChannelEncryptRequest();
		
		msg.protocolVersion = buf.getInt();
		msg.universe = EUniverse.lookupUniverse( buf.getInt() );
		
		return msg;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( protocolVersion );
		serialBuffer.putInt( universe.getCode() );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
