package net.steam3.packets;

import java.nio.ByteBuffer;

import com.google.protobuf.InvalidProtocolBufferException;

import steamkit.steam3.SteamLanguage.ISteamSerializableHeader;
import steamkit.steam3.SteamLanguage.ISteamSerializableMessage;

public class ClientMsg<X extends ISteamSerializableMessage, Y extends ISteamSerializableHeader>
{
	private Y header;
	private X msg;

	private ByteBuffer data;
	
	public ClientMsg(Class<X> msgClass, Class<Y> headerClass)
	{
		try {
			this.header = headerClass.getConstructor().newInstance();
			this.msg = msgClass.getConstructor().newInstance();
		} catch (Exception e) {
			e.printStackTrace();
		}
		
		this.header.SetEMsg( this.msg.GetEMsg() );
		this.data = null;
	}
	
	public ClientMsg(Class<X> msgClass, Class<Y> headerClass, ByteBuffer input) throws InvalidProtocolBufferException
	{
		this(msgClass, headerClass);
		
		header.deserialize(input);
		msg.deserialize(input);
		
		byte[] data = new byte[input.remaining()];
		input.get(data);
		
		this.data = ByteBuffer.wrap( data );
	}
	
	public Y getHeader()
	{
		return header;
	}
	
	public X getMsg()
	{
		return msg;
	}
	
	public void setPayload( ByteBuffer payload )
	{
		this.data = payload;
	}
	
	public ByteBuffer getPayload()
	{
		return this.data;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer header = this.header.serialize();
		ByteBuffer msg = this.msg.serialize();
		
		int serializedSize = header.limit() + msg.limit();
		
		if ( data != null )
		{
			serializedSize += data.limit();
		}
		
		ByteBuffer combined = ByteBuffer.allocate( serializedSize );
		combined.put( header );
		combined.put( msg );
		
		if ( data != null )
		{
			combined.put( data );
		}
		
		combined.flip();
		return combined;
	}
}
