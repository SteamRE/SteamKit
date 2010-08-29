package net.steam3.packets;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class MsgHdr implements ISerializable
{
	private static final int Size = 20;
	
	private EMsg msg;
	
	private long targetJobID;
	private long sourceJobID;
	
	public MsgHdr()
	{
		msg = EMsg.Invalid;
		
		targetJobID = -1;
		sourceJobID = -1;
	}
	
	public void initialize( EMsg msg )
	{
		this.msg = msg;
	}
	
	public static MsgHdr deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static MsgHdr deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		MsgHdr header = new MsgHdr();
		
		header.msg = EMsg.lookupMsg( buf.getInt() );
		
		header.targetJobID = buf.getLong();
		header.sourceJobID = buf.getLong();
		
		return header;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( msg.getCode() );
		serialBuffer.putLong( targetJobID );
		serialBuffer.putLong( sourceJobID );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
