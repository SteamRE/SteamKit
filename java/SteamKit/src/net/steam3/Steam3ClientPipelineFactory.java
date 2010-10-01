package net.steam3;

import static org.jboss.netty.channel.Channels.*;

import org.jboss.netty.channel.ChannelPipeline;
import org.jboss.netty.channel.ChannelPipelineFactory;

public class Steam3ClientPipelineFactory implements ChannelPipelineFactory
{
	IUDPHandlerCallback recvcallbackholder;
	
	public Steam3ClientPipelineFactory( IUDPHandlerCallback recvcallback )
	{
		this.recvcallbackholder = recvcallback;
	}
	
	public ChannelPipeline getPipeline() throws Exception
	{
		ChannelPipeline pipeline = pipeline();
		
		pipeline.addLast( "framer", new Steam3FrameDecoder() );
		pipeline.addLast( "handler", new Steam3ClientHandler( recvcallbackholder ) );
		
		return pipeline;
	}

}
