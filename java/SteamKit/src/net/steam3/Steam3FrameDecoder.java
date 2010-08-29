package net.steam3;

import net.steam3.packets.udp.*;
import net.steam3.types.Steam3FrameState;

import org.jboss.netty.buffer.ChannelBuffer;
import org.jboss.netty.channel.Channel;
import org.jboss.netty.channel.ChannelHandlerContext;
import org.jboss.netty.handler.codec.replay.ReplayingDecoder;

public class Steam3FrameDecoder extends ReplayingDecoder<Steam3FrameState>
{
	private UDPPacket packet;
	
	public Steam3FrameDecoder()
	{
		super( Steam3FrameState.READ_HEADER );
	}
	
	protected Object decode( ChannelHandlerContext ctx, Channel channel, ChannelBuffer buf, Steam3FrameState state ) throws Exception
    {
		//switch( state )
		{
		//case READ_HEADER:
			packet = UDPPacket.deserialize( buf );
			
			//checkpoint( Steam3FrameState.READ_MSG );
		
		//case READ_MSG:
			Object result = null;
			
			switch( packet.getType() )
			{
			case Challenge:
				result = Challenge.deserialize( buf );
				break;
				
			case Accept:
				result = new Accept();
				break;
				
			case Data:
				ChannelBuffer data = buf.readBytes( packet.getPayloadSize() );
				result = Data.deserialize( data );
				break;
				
			case Datagram:
				result = new Datagram();
				break;
				
			case Disconnect:
				result = new Disconnect();
				break;
			}
			
			//checkpoint( Steam3FrameState.READ_HEADER );
			return new Object[] { packet, result };
			
		//default:
			//throw new Error( "Unhandled state" );
		}
    }
}
