package net.steam3;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import com.google.protobuf.InvalidProtocolBufferException;

import steamkit.steam3.SteamLanguage.EUdpPacketType;
import steamkit.steam3.SteamLanguage.UdpHeader;

public class UDPPacket
{
	private UdpHeader header;
	
	private ByteBuffer payload;
	
	public UDPPacket()
	{
		header = new UdpHeader();
		this.payload = null;
	}
	
	public UDPPacket( ByteBuffer buf ) throws InvalidProtocolBufferException
	{
		this();
		header.deserialize( buf );
		byte[] payload = new byte[ buf.remaining() ];
		buf.get(payload);
		this.payload = ByteBuffer.wrap(payload);
		this.payload.order( ByteOrder.LITTLE_ENDIAN );
	}
	
	public void initialize( EUdpPacketType type, int seq, int ackseq )
	{
		header.setPacketType( type );
		header.setSeqThis( seq );
		header.setSeqAck( ackseq );
	}
	
	public int getPayloadSize()
	{
		return payload.limit();
	}
	
	public UdpHeader getHeader()
	{
		return header;
	}
	
	public ByteBuffer getPayload()
	{
		return payload;
	}
	
	public void setPayload( ByteBuffer payload )
	{
		this.payload = payload;
		this.header.setPayloadSize( (short)payload.limit() );
		this.header.setMsgSize(payload.limit() );
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer header = this.header.serialize();
		int payloadSize = 0;
		
		if( this.payload != null)
		{
			payloadSize = this.payload.limit();
		}

		ByteBuffer finalbuf = ByteBuffer.allocate(payloadSize + header.limit());
		finalbuf.order( ByteOrder.LITTLE_ENDIAN );
		
		finalbuf.put( header );
		
		if ( this.payload != null )
		{
			finalbuf.put( this.payload );
		}
		
		finalbuf.flip();
		return finalbuf;
	}
}
