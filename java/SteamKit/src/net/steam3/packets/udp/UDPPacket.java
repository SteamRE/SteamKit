package net.steam3.packets.udp;

import net.steam3.types.EUdpPacketType;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.util.ISerializable;

public class UDPPacket implements ISerializable
{
	public static final int PACKET_MAGIC = 0x31305356;
	private static final int Size = 36;
	
	private int magic;
	private short payloadSize;
	private EUdpPacketType type;
	private byte flags;
	
	private int srcID, dstID;
	private int thisSeq, ackSeq;
	
	private int packetCount, startSeq;
	
	private int msgLength;
	
	private ByteBuffer payload;
	
	public UDPPacket()
	{
		this.magic = PACKET_MAGIC;
		this.payloadSize = 0;
		this.type = EUdpPacketType.Invalid;
		this.flags = 0;
		this.srcID = 0x200;
		this.dstID = 0;
		this.thisSeq = 0;
		this.ackSeq = 0;
		this.packetCount = 0;
		this.startSeq = 0;
		this.msgLength = 0;
		
		this.payload = null;
	}
	
	public void initialize( EUdpPacketType type, int seq, int ackseq )
	{
		setType( type );
		setSequence( seq, ackseq );
	}
	
	public short getPayloadSize()
	{
		return payloadSize;
	}
	
	public EUdpPacketType getType()
	{
		return type;
	}
	
	public void setType( EUdpPacketType type )
	{
		this.type = type;
	}
	
	public int getSourceConnID()
	{
		return this.srcID;
	}
	
	public void setTargetConnID( int connID )
	{
		this.dstID = connID;
	}
	
	public int getSequence()
	{
		return thisSeq;
	}
	
	public void setSequence( int seq, int ackseq )
	{
		this.thisSeq = seq;
		this.ackSeq = ackseq;
	}
	
	public void setCounts( int packets, int startSeq )
	{
		this.packetCount = packets;
		this.startSeq = startSeq;
	}
	
	public ByteBuffer getPayload()
	{
		return payload;
	}
	
	public void setPayload( ByteBuffer payload )
	{
		this.payload = payload;
		this.payloadSize = (short)payload.limit();
		this.msgLength = this.payloadSize;
	}
	
	public static UDPPacket deserialize( ChannelBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		UDPPacket packet = new UDPPacket();
		
		int availableLength = buf.writerIndex();
		
		packet.magic = buf.readInt();
		
		if ( packet.magic != PACKET_MAGIC )
		{
			throw new Error("Packet magic invalid");
		}
		
		packet.payloadSize = buf.readShort();
		
		int inferredHeaderLength = availableLength - packet.payloadSize;
		
		assert inferredHeaderLength == Size; // old proto
		
		packet.type = EUdpPacketType.lookupType( buf.readByte() );
		packet.flags = buf.readByte();
		
		packet.srcID = buf.readInt();
		packet.dstID = buf.readInt();
		
		packet.thisSeq = buf.readInt();
		packet.ackSeq = buf.readInt();
		
		packet.packetCount = buf.readInt();
		packet.startSeq = buf.readInt();
		
		packet.msgLength = buf.readInt();
		
		return packet;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer;
		
		if( payload != null )
		{
			serialBuffer = ByteBuffer.allocate( Size + payload.limit() );
		}
		else
		{
			serialBuffer = ByteBuffer.allocate( Size );
		}
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( magic );
		serialBuffer.putShort( payloadSize );
		serialBuffer.put( (byte)type.getCode() );
		serialBuffer.put( flags );
		serialBuffer.putInt( srcID );
		serialBuffer.putInt( dstID );
		serialBuffer.putInt( thisSeq );
		serialBuffer.putInt( ackSeq );
		serialBuffer.putInt( packetCount );
		serialBuffer.putInt( startSeq );
		serialBuffer.putInt( msgLength );
		
		if ( payload != null )
		{
			serialBuffer.put( payload );
		}
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
