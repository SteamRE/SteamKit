package net.steam3;

import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.atomic.AtomicInteger;
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
	private ConnectionlessBootstrap b;
	private DatagramChannel c;

	private IUDPCallbacks callbacks;

	private ISteam3Filter netfilter;
	private byte[] sessionKey;
	
	private AtomicInteger lastAckSeq;
	private AtomicInteger thisSeq;
	
	private int remoteConnID;
	
	public UDPConnection( ExecutorService es, IUDPCallbacks callbacks )
	{
		this.callbacks = callbacks;

		netfilter = new PassthruFilter();
		
		lastAckSeq = new AtomicInteger( 0 );
		thisSeq = new AtomicInteger( 0 );
		
		NioDatagramChannelFactory f = new NioDatagramChannelFactory( es );
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
		lastAckSeq.set( packet.getSequence() );
		
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
					
					callbacks.RecvDataCallback( fragData, remoteEndpoint );
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
        }
	}
	
	private void SendUDPMessage( ISerializable msg, InetSocketAddress endpoint )
	{
		c.write( wrappedBuffer( msg.serialize() ), endpoint );
	}
	
	private void SendMessage( ISerializable msg, InetSocketAddress endpoint )
	{
		UDPPacket data = new UDPPacket();
		data.initialize( EUdpPacketType.Data, thisSeq.incrementAndGet(), lastAckSeq.get() );
		data.setTargetConnID( remoteConnID );
		data.setCounts( 1, thisSeq.get() );
		
		data.setPayload( netfilter.processOutgoing( msg.serialize() ) );
		
		SendUDPMessage( data, endpoint );
	}
	
	public void SendAnonLogOn( Steam3Session session, CSteamID steamid )
	{
		ClientAnonLogOn logon = new ClientAnonLogOn();
		logon.initialize();
		
		ExtendedClientMsgHdr exthdr = new ExtendedClientMsgHdr();
		exthdr.initialize( logon.getMsg() );
		exthdr.setSteamID( steamid );
		
		ClientMsg logonMsg = new ClientMsg( exthdr, logon );
		
		SendMessage( logonMsg, session.getEndpoint() );
	}
	
	public void SendConnect( Steam3Session session, int challenge )
	{
		UDPPacket connectreq = new UDPPacket();
		connectreq.initialize( EUdpPacketType.Connect, thisSeq.incrementAndGet(), lastAckSeq.get() );
		connectreq.setTargetConnID( remoteConnID );
		connectreq.setCounts( 1, thisSeq.get() );
		
		ConnectChallenge connectchal = new ConnectChallenge();
		connectchal.setChallenge( challenge ^ ConnectChallenge.CHALLENGE_MASK );
		
		connectreq.setPayload( connectchal.serialize() );
		
		SendUDPMessage( connectreq, session.getEndpoint() );
	}
	
	public void SendHeartbeat( Steam3Session session )
	{
		ClientHeartBeat heartbeat = new ClientHeartBeat();
		
		ExtendedClientMsgHdr exthdr = new ExtendedClientMsgHdr();
		exthdr.initialize( heartbeat.getMsg() );
		session.FillExtHdr( exthdr );
		
		SendMessage( heartbeat, session.getEndpoint() );
	}
	
	public void SendChallengeRequest( InetSocketAddress endpoint )
	{
		UDPPacket challengereq = new UDPPacket();
		challengereq.initialize( EUdpPacketType.ChallengeReq, 1, lastAckSeq.get() );

		SendUDPMessage( challengereq, endpoint );
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
