package net.steam3;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import net.steam3.types.Steam3FrameState;

import org.jboss.netty.buffer.ChannelBuffer;
import org.jboss.netty.channel.Channel;
import org.jboss.netty.channel.ChannelHandlerContext;
import org.jboss.netty.handler.codec.frame.FrameDecoder;
import org.jboss.netty.handler.codec.replay.ReplayingDecoder;

import steamkit.steam3.SteamLanguage;
import steamkit.steam3.SteamLanguage.ChallengeData;
import steamkit.util.Logger;

public class Steam3FrameDecoder extends FrameDecoder
{
	public Steam3FrameDecoder()
	{
		super();
	}
	
	protected Object decode( ChannelHandlerContext ctx, Channel channel, ChannelBuffer buf ) throws Exception
    {
		if (buf.readableBytes() < 36)
		{
			return null;
		}
		
		UDPPacket packet = null;
		ByteBuffer bytebuf = buf.readBytes( buf.readableBytes() ).toByteBuffer();
		bytebuf.order( ByteOrder.LITTLE_ENDIAN );
		
		try
		{
			packet = new UDPPacket( bytebuf );
		}
		catch(Exception e)
		{
			Logger.getLogger().println("Got invalid packet");
			return null;
		}

		Object result = null;
		
		switch( packet.getHeader().getPacketType() )
		{
		case Challenge:
			result = new ChallengeData();
			((ChallengeData)result).deserialize( packet.getPayload() );
			break;
			
		case Accept:
			result = new SteamLanguage.Accept();
			break;
			
		case Data:
			result = new Data();
			((Data)result).deserialize( packet.getPayload() );
			break;
			
		case Datagram:
			result = new SteamLanguage.Datagram();
			break;
			
		case Disconnect:
			result = new SteamLanguage.Disconnect();
			break;
		}
		
		return new Object[] { packet, result };
    }
}
