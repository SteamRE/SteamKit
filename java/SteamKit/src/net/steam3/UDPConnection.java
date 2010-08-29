package net.steam3;

import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.zip.CRC32;
import java.util.zip.Checksum;

import net.steam3.filter.EncryptionFilter;
import net.steam3.filter.ISteam3Filter;
import net.steam3.filter.PassthruFilter;
import net.steam3.packets.*;
import net.steam3.packets.msg.*;
import net.steam3.packets.udp.*;
import net.steam3.types.EUdpPacketType;

import static org.jboss.netty.buffer.ChannelBuffers.*;

import org.jboss.netty.bootstrap.ConnectionlessBootstrap;
import org.jboss.netty.buffer.HeapChannelBufferFactory;
import org.jboss.netty.channel.socket.DatagramChannel;
import org.jboss.netty.channel.socket.nio.NioDatagramChannelFactory;

import steamkit.types.CSteamID;
import steamkit.types.EResult;
import steamkit.util.CryptoHelper;
import steamkit.util.ISerializable;
import steamkit.util.UniverseKey;

public class UDPConnection implements IUDPHandlerCallback
{
	private static final ExecutorService es = Executors.newCachedThreadPool();
	private static final NioDatagramChannelFactory f = new NioDatagramChannelFactory( es );
	
	private ConnectionlessBootstrap b;
	private DatagramChannel c;

	private IUDPCallbacks callbacks;

	private ISteam3Filter netfilter;
	private byte[] sessionKey;
	
	private int lastAckSeq;
	private int thisSeq;
	
	private int remoteConnID;
	
	public UDPConnection( IUDPCallbacks callbacks )
	{
		this.callbacks = callbacks;
	
		netfilter = new PassthruFilter();
		
		lastAckSeq = 0;
		thisSeq = 0;
		
		b = new ConnectionlessBootstrap( f );
		b.setPipelineFactory( new Steam3ClientPipelineFactory( this ) );

		b.setOption( "broadcast", "false" );
		b.setOption( "sendBufferSize", 15000 );
		b.setOption( "receiveBufferSize", 15000 );
		b.setOption( "bufferFactory", new
				HeapChannelBufferFactory( ByteOrder.LITTLE_ENDIAN ) );


		c = (DatagramChannel) b.bind( new InetSocketAddress( 27001 ) );
	}
	
	public void close()
	{
		c.close().awaitUninterruptibly();
		b.releaseExternalResources();
	}
	
	public void UDPRecvCallback( UDPPacket packet, Object message, InetSocketAddress remoteEndpoint )
	{
		lastAckSeq = packet.getSequence();
		
        if ( message instanceof Challenge )
        {
        	callbacks.ChalllengeResponseCallback( (Challenge)message, remoteEndpoint );
        }
        else if ( message instanceof Data )
        {
			Data data = (Data)message;
			
			if ( !(netfilter instanceof PassthruFilter) )
			{
				data = Data.deserialize( netfilter.processIncoming( data.getBuffer() ) );
			}
			
			ByteBuffer buf = data.getBuffer();
			
			switch( data.peekType() )
			{
			case ChannelEncryptRequest:
				MsgHdr.deserialize( buf );
				ChannelEncryptRequest encreq = ChannelEncryptRequest.deserialize( buf );
				
				SendEncryptRequestResponse( encreq, remoteEndpoint );
				
				callbacks.SessionNegotiationInitiated( encreq.getUniverse() );
				break;
				
			case ChannelEncryptResult:
				MsgHdr.deserialize( buf );
				ChannelEncryptResult encres = ChannelEncryptResult.deserialize( buf );
				
				if ( encres.getResult() == EResult.OK )
				{
					netfilter = new EncryptionFilter( sessionKey );
				}
				
				callbacks.SessionNegotiationCompleted( encres.getResult() );
				break;
			
			case Multi:
				MsgHdr.deserialize( buf );
				MultiMsg multimsg = MultiMsg.deserialize( buf );
				
				if ( multimsg.getLength() > 0 )
				{
					// decompress
				}
				
				while ( buf.position() < buf.limit() )
				{
					int fragLength = buf.getInt();
					byte[] fragBuf = new byte[fragLength];
					buf.get( fragBuf );
					
					Data fragData = Data.deserialize( fragBuf );
					System.out.println( "Multi peek: " + fragData.peekType() + " length: " + fragLength );
				}
				
				break;
			}
			
        	callbacks.RecvDataCallback( data, remoteEndpoint );
        }
        else if ( message instanceof Accept )
        {
        	remoteConnID = packet.getSourceConnID();
        	
        	callbacks.AcceptCallback( (Accept)message );
        }
        else if ( message instanceof Disconnect )
        {
        	callbacks.DisconnectCallback( (Disconnect)message );
        	
        	close();
        }
	}
	
	private void SendUDPMessage( ISerializable msg, InetSocketAddress endpoint )
	{
		c.write( wrappedBuffer( msg.serialize() ), endpoint );
	}
	
	private void SendMessage( ISerializable msg, InetSocketAddress endpoint )
	{
		UDPPacket data = new UDPPacket();
		data.initialize( EUdpPacketType.Data, ++thisSeq, lastAckSeq );
		data.setTargetConnID( remoteConnID );
		data.setCounts( 1, thisSeq );
		
		data.setPayload( netfilter.processOutgoing( msg.serialize() ) );
		
		SendUDPMessage( data, endpoint );
	}
	
	public void SendAnonLogOn( CSteamID steamid, InetSocketAddress endpoint )
	{
		ClientAnonLogOn logon = new ClientAnonLogOn();
		logon.initialize();
		
		ExtendedClientMsgHdr exthdr = new ExtendedClientMsgHdr();
		exthdr.initialize( logon.getMsg() );
		exthdr.setSteamID( steamid );
		
		ClientMsg logonMsg = new ClientMsg( exthdr, logon );
		
		SendMessage( logonMsg, endpoint );
	}
	
	public void SendChallengeRequest( InetSocketAddress endpoint )
	{
		UDPPacket challengereq = new UDPPacket();
		challengereq.initialize( EUdpPacketType.ChallengeReq, 1, lastAckSeq );

		SendUDPMessage( challengereq, endpoint );
	}

	public void SendConnect( int challenge, InetSocketAddress endpoint )
	{
		UDPPacket connectreq = new UDPPacket();
		connectreq.initialize( EUdpPacketType.Connect, ++thisSeq, lastAckSeq );
		connectreq.setTargetConnID( remoteConnID );
		connectreq.setCounts( 1, thisSeq );
		
		ConnectChallenge connectchal = new ConnectChallenge();
		connectchal.setChallenge( challenge ^ ConnectChallenge.CHALLENGE_MASK );
		
		connectreq.setPayload( connectchal.serialize() );
		
		SendUDPMessage( connectreq, endpoint );
	}
	
	
	private void SendEncryptRequestResponse( ChannelEncryptRequest encreq, InetSocketAddress endpoint )
	{
		sessionKey = CryptoHelper.GenerateRandomBlock( 32 );
		
		UniverseKey unikey = UniverseKey.GetKey( encreq.getUniverse() );
		byte[] key = unikey.getKey();
		
		byte[] encryptedSession = CryptoHelper.RSAEncrypt( sessionKey, key );

		ChannelEncryptResponse response = new ChannelEncryptResponse();
		response.initialize( encryptedSession.length );
		
		MsgHdr header = new MsgHdr();
		header.initialize( response.getMsg() );
		
		ClientMsg encResponse = new ClientMsg( header, response );

		Checksum checksum = new CRC32();
		checksum.update( encryptedSession, 0, encryptedSession.length );
		
		ByteBuffer payload = ByteBuffer.allocate( encryptedSession.length + 8 );
		payload.order( ByteOrder.LITTLE_ENDIAN );
		
		payload.put( encryptedSession );
		payload.putInt( (int)checksum.getValue() );
		payload.putInt( 0 );
		
		payload.flip();
		encResponse.setPayload( payload );
		
		SendMessage( encResponse, endpoint );
	}
}
