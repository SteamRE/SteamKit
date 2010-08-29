package net.steam3.packets;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.types.CSteamID;
import steamkit.util.ISerializable;

public class ExtendedClientMsgHdr implements ISerializable
{
	private static final int Size = 36;
	private static final int HeaderVersion = 2;
	private static final byte Canary = (byte)239;
	
	private EMsg msg;
	
    public byte headerSize;

    public short headerVersion;

    public long targetJobID;
    public long sourceJobID;

    public byte headerCanary;

    public long steamID;

    public int sessionID;
	
	public ExtendedClientMsgHdr()
	{
        msg = EMsg.Invalid;
        headerSize = 0;
        headerVersion = 0;

        this.targetJobID = -1;
        this.sourceJobID = -1;

        this.headerCanary = 0;

        this.steamID = 0;
        this.sessionID = 0;
	}
	
	public void initialize( EMsg msg )
	{
		this.msg = msg;
		headerSize = Size;
		headerVersion = HeaderVersion;
		headerCanary = Canary;
	}
	
	public int getSessionID()
	{
		return sessionID;
	}
	
	public void setSessionID( int sessionID )
	{
		this.sessionID = sessionID;
	}
	
	public CSteamID getSteamID()
	{
		return new CSteamID( steamID );
	}
	
	public void setSteamID( CSteamID steamid )
	{
		this.steamID = steamid.getLong();
	}
	
	public static ExtendedClientMsgHdr deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static ExtendedClientMsgHdr deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ExtendedClientMsgHdr header = new ExtendedClientMsgHdr();
		
		header.msg = EMsg.lookupMsg( buf.getInt() );
		header.headerSize = buf.get();
		
		assert header.headerSize == Size;
		
		header.headerVersion = buf.getShort();
		header.targetJobID = buf.getLong();
		header.sourceJobID = buf.getLong();
		header.headerCanary = buf.get();
		header.steamID = buf.getLong();
		header.sessionID = buf.getInt();
		
		return header;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( msg.getCode() );
		serialBuffer.put( headerSize );
		serialBuffer.putShort( headerVersion );
		serialBuffer.putLong( targetJobID );
		serialBuffer.putLong( sourceJobID );
		serialBuffer.put( headerCanary );
		serialBuffer.putLong( steamID );
		serialBuffer.putInt( sessionID );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
