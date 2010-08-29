package net.steam3.packets;

import java.nio.ByteBuffer;

import steamkit.util.ISerializable;

public class ClientMsg implements ISerializable
{
	private ISerializable header;
	private ISerializable msg;

	private ByteBuffer data;
	
	public ClientMsg(ISerializable header, ISerializable msg)
	{
		this.header = header;
		this.msg = msg;
		this.data = null;
	}
	
	public void setPayload( ByteBuffer payload )
	{
		this.data = payload;
	}
	
	public ByteBuffer serialize()
	{
		ISerializable header = (ISerializable)this.header;
		ISerializable msg = (ISerializable)this.msg;
		
		ByteBuffer headerbuf = header.serialize();
		ByteBuffer msgbuf = msg.serialize();
		
		int serializedSize = headerbuf.limit() + msgbuf.limit();
		
		if ( data != null )
		{
			serializedSize += data.limit();
		}
		
		ByteBuffer combined = ByteBuffer.allocate( serializedSize );
		combined.put( headerbuf );
		combined.put( msgbuf );
		
		if ( data != null )
		{
			combined.put( data );
		}
		
		combined.flip();
		return combined;
	}
}
