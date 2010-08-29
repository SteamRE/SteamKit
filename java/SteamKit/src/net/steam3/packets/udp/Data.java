package net.steam3.packets.udp;

import net.steam3.types.EMsg;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class Data implements ISerializable
{
	private EMsg peekmsg;
	private ByteBuffer buffer;
	
	public Data()
	{
		peekmsg = EMsg.Invalid;
		buffer = null;
	}

	public EMsg peekType()
	{
		return peekmsg;
	}
	
	public ByteBuffer getBuffer()
	{
		return buffer;
	}
	
	public static Data deserialize( byte[] buf )
	{
		ByteBuffer wrapper = ByteBuffer.wrap( buf );
		wrapper.order( ByteOrder.LITTLE_ENDIAN );
		
		return deserialize( wrapper );
	}
	
	public static Data deserialize( ChannelBuffer buf )
	{
		ByteBuffer wrapper = buf.toByteBuffer();
		wrapper.order( ByteOrder.LITTLE_ENDIAN );
		
		return deserialize( wrapper );
	}
	
	public static Data deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		Data data = new Data();
		
		data.buffer = buf;
		
		buf.mark();
		data.peekmsg = EMsg.lookupMsg( buf.getInt() );
		buf.reset();
		
		return data;
	}
	
	public ByteBuffer serialize()
	{
		if ( buffer == null )
		{
			return null;
		}
		
		ByteBuffer serialBuffer = ByteBuffer.allocate( buffer.limit() );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.put( buffer );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
