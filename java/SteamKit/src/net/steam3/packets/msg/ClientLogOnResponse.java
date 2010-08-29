package net.steam3.packets.msg;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.EMsg;

import org.jboss.netty.buffer.ChannelBuffer;

import steamkit.types.EResult;

public class ClientLogOnResponse
{
	private static final int Size = 28;

    private EResult result;
    
    private int outOfGameHeartbeatRateSec;
    private int inGameHeartbeatRateSec;
    
    private long clientSuppliedSteamId;

    private int publicIP;

    private int serverRealTime;
    
    public ClientLogOnResponse()
    {
    	result = EResult.Invalid;
    	outOfGameHeartbeatRateSec = 0;
    	inGameHeartbeatRateSec = 0;
    	clientSuppliedSteamId = 0;
    	publicIP = 0;
    	serverRealTime = 0;
    }
	
	public EMsg getMsg()
	{
		return EMsg.ClientLogOnResponse;
	}
	
	public EResult getResult()
	{
		return result;
	}
	
	public static ClientLogOnResponse deserialize( ChannelBuffer buf )
	{
		return deserialize( buf.toByteBuffer() );
	}
	
	public static ClientLogOnResponse deserialize( ByteBuffer buf )
	{
		assert buf.order() == ByteOrder.LITTLE_ENDIAN;
		
		ClientLogOnResponse msg = new ClientLogOnResponse();
		
		msg.result = EResult.lookupResult( buf.getInt() );
		msg.outOfGameHeartbeatRateSec = buf.getInt();
		msg.inGameHeartbeatRateSec = buf.getInt();
		msg.clientSuppliedSteamId = buf.getLong();
		msg.publicIP = buf.getInt();
		msg.serverRealTime = buf.getInt();
		
		return msg;
	}
	
	public ByteBuffer serialize()
	{
		ByteBuffer serialBuffer = ByteBuffer.allocate( Size );
		
		serialBuffer.order( ByteOrder.LITTLE_ENDIAN );
		
		serialBuffer.putInt( result.getCode() );
		serialBuffer.putInt( outOfGameHeartbeatRateSec );
		serialBuffer.putInt( inGameHeartbeatRateSec );
		serialBuffer.putLong( clientSuppliedSteamId );
		serialBuffer.putInt( publicIP );
		serialBuffer.putInt( serverRealTime );
		
		serialBuffer.flip();
		return serialBuffer;
	}
}
