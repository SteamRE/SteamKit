package net.steam3.packets.msg;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import org.jboss.netty.buffer.ChannelBuffer;

import net.steam3.types.EMsg;
import steamkit.util.ISerializable;

public class ChannelEncryptResponse implements ISerializable
{
	private static final int Size = 8;
	private static final int ProtocolVersion = 1;

    private int protocolVersion;
    private int keySize;
    
    public ChannelEncryptResponse()
    {
    	protocolVersion = 0;
    	keySize = 0;
    }
    
	public void initialize( int keySize )
	{
		protocolVersion = ProtocolVersion;
		this.keySize = keySize;
	}
	
	public EMsg getMsg()
	{
		return EMsg.ChannelEncryptResponse;
	}
	
	public static ChannelEncryptResponse deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static ChannelEncryptResponse deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ChannelEncryptResponse msg = new ChannelEncryptResponse();
		
		msg.protocolVersion = buf.getInt();
		msg.keySize = buf.getInt();
		
		return msg;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( protocolVersion );
		serialBuffer.putInt( keySize );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
