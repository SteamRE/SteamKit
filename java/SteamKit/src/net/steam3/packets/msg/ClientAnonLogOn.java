package net.steam3.packets.msg;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class ClientAnonLogOn implements ISerializable
{
	private static final int Size = 12;
	private static final int ObfuscationMask = 0xBAADF00D;
	private static final int ProtocolVersion = 65563;

    private int protocolVersion;
    
    private int privateIPObfuscated;
    private int publicIP;
    
    public ClientAnonLogOn()
    {
    	protocolVersion = 0;
    	privateIPObfuscated = 0;
    	publicIP = 0;
    }
    
	public void initialize()
	{
		protocolVersion = ProtocolVersion;
	}
	
	public EMsg getMsg()
	{
		return EMsg.ClientAnonLogOn;
	}
	
	public static ClientAnonLogOn deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static ClientAnonLogOn deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ClientAnonLogOn msg = new ClientAnonLogOn();
		
		msg.protocolVersion = buf.getInt();
		msg.privateIPObfuscated = buf.getInt();
		msg.publicIP = buf.getInt();
		
		return msg;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( protocolVersion );
		serialBuffer.putInt( privateIPObfuscated );
		serialBuffer.putInt( publicIP );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
