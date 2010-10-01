package net.steam3;

import java.net.InetSocketAddress;

import org.jboss.netty.channel.ChannelHandlerContext;
import org.jboss.netty.channel.ExceptionEvent;
import org.jboss.netty.channel.MessageEvent;
import org.jboss.netty.channel.SimpleChannelHandler;

public class Steam3ClientHandler extends SimpleChannelHandler
{
	private IUDPHandlerCallback recvcallback;

	public Steam3ClientHandler( IUDPHandlerCallback recvcallback )
	{
		this.recvcallback = recvcallback;
	}
	
    @Override
    public void messageReceived( ChannelHandlerContext ctx, MessageEvent e )
    {
        Object[] pmessage = (Object[])e.getMessage();
        
        UDPPacket header = (UDPPacket)pmessage[0];
        Object message = pmessage[1];
        
        recvcallback.UDPRecvCallback( header, message, (InetSocketAddress)e.getRemoteAddress() );
    }

    @Override
    public void exceptionCaught( ChannelHandlerContext ctx, ExceptionEvent e )
    {
        e.getCause().printStackTrace();
        e.getChannel().close();
    }


}
