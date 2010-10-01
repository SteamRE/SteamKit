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
import net.steam3.packets.ClientMsg;
import net.steam3.packets.ClientMsgProtobuf;

import static org.jboss.netty.buffer.ChannelBuffers.*;

import org.jboss.netty.bootstrap.ConnectionlessBootstrap;
import org.jboss.netty.buffer.HeapChannelBufferFactory;
import org.jboss.netty.channel.socket.DatagramChannel;
import org.jboss.netty.channel.socket.nio.NioDatagramChannelFactory;

import com.google.protobuf.InvalidProtocolBufferException;

import steamkit.steam3.SteamLanguage.Accept;
import steamkit.steam3.SteamLanguage.ChallengeData;
import steamkit.steam3.SteamLanguage.ConnectData;
import steamkit.steam3.SteamLanguage.Disconnect;
import steamkit.steam3.SteamLanguage.EAccountType;
import steamkit.steam3.SteamLanguage.EResult;
import steamkit.steam3.SteamLanguage.EUdpPacketType;
import steamkit.steam3.SteamLanguage.EUniverse;
import steamkit.steam3.SteamLanguage.MsgChannelEncryptRequest;
import steamkit.steam3.SteamLanguage.MsgChannelEncryptResponse;
import steamkit.steam3.SteamLanguage.MsgChannelEncryptResult;
import steamkit.steam3.SteamLanguage.MsgClientHeartBeat;
import steamkit.steam3.SteamLanguage.MsgClientLogon;
import steamkit.steam3.SteamLanguage.MsgHdr;
import steamkit.steam3.SteamLanguage.MsgMulti;
import steamkit.steam3.SteamMessages.CMsgClientLogon;
import steamkit.steam3.SteamMessages.CMsgProtoBufHeader;
import steamkit.types.CSteamID;
import steamkit.util.CryptoHelper;
import steamkit.util.Logger;
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
		lastAckSeq.set( packet.getHeader().getSeqThis() );
		
        if ( message instanceof ChallengeData )
        {
        	callbacks.ChalllengeResponseCallback( (ChallengeData)message, remoteEndpoint );
        }
        else if ( message instanceof Data )
        {
			Data data = (Data)message;
			
			if ( !(netfilter instanceof PassthruFilter) )
			{
				Data decrypted = new Data();
				decrypted.deserialize( netfilter.processIncoming( data.getBuffer() ) );
				data = decrypted;
			}
			
			int curSeq = thisSeq.get();
			
			ByteBuffer buf = data.getBuffer();
			
			switch( data.peekType() )
			{
			case ChannelEncryptRequest:
				ClientMsg<MsgChannelEncryptRequest, MsgHdr> encreq = null;
				try {
					encreq = new ClientMsg<MsgChannelEncryptRequest, MsgHdr>(MsgChannelEncryptRequest.class, MsgHdr.class, buf);
				} catch (InvalidProtocolBufferException e) {
					e.printStackTrace();
				}

				SendEncryptRequestResponse( encreq.getMsg(), remoteEndpoint );
				
				callbacks.SessionNegotiationInitiated( encreq.getMsg().getUniverse() );
				break;
				
			case ChannelEncryptResult:
				ClientMsg<MsgChannelEncryptResult, MsgHdr> encres = null;
				try {
					encres = new ClientMsg<MsgChannelEncryptResult, MsgHdr>(MsgChannelEncryptResult.class, MsgHdr.class, buf);
				} catch (InvalidProtocolBufferException e) {
					e.printStackTrace();
				}

				if ( encres.getMsg().getResult() == EResult.OK )
				{
					netfilter = new EncryptionFilter( sessionKey );
				}
				
				callbacks.SessionNegotiationCompleted( encres.getMsg().getResult() );
				break;
				
			case Multi:
				ClientMsgProtobuf<MsgMulti> multi = null;
				try {
					multi = new ClientMsgProtobuf<MsgMulti>(MsgMulti.class, buf);
				} catch (InvalidProtocolBufferException e) {
					e.printStackTrace();
				}
				
				ByteBuffer multibuf = multi.getMsg().getProto().getMessageBody().asReadOnlyByteBuffer();
				multibuf.order( ByteOrder.LITTLE_ENDIAN );
				
				if ( multi.getMsg().getProto().getSizeUnzipped() > 0 )
				{
					// decompress
				}
				
				while ( multibuf.position() < multibuf.limit() )
				{
					int fragLength = multibuf.getInt();
					byte[] fragBuf = new byte[fragLength];
					multibuf.get( fragBuf );
					
					ByteBuffer wrapper = ByteBuffer.wrap(fragBuf);
					wrapper.order( ByteOrder.LITTLE_ENDIAN );
					
					Data fragData = new Data();
					fragData.deserialize( wrapper );
					
					callbacks.RecvDataCallback( fragData, remoteEndpoint );
				}
				
				break;
			}
			
        	callbacks.RecvDataCallback( data, remoteEndpoint );
        	
        	// send datagram ack if we don't reply
        	if ( thisSeq.get() == curSeq )
        	{
        		SendAck( remoteEndpoint );
        	}
        }
        else if ( message instanceof Accept )
        {
        	remoteConnID = packet.getHeader().getSourceConnID();
        	
        	callbacks.AcceptCallback( (Accept)message );
        }
        else if ( message instanceof Disconnect )
        {
        	callbacks.DisconnectCallback( (Disconnect)message );
        }
	}
	
	private void SendUDPMessage( ByteBuffer msg, InetSocketAddress endpoint )
	{
		c.write( wrappedBuffer( msg ), endpoint );
	}
	
	private void SendAck( InetSocketAddress endpoint )
	{
		UDPPacket ack = new UDPPacket();
		ack.initialize( EUdpPacketType.Datagram, 0, lastAckSeq.get() );
		ack.getHeader().setDestConnID( remoteConnID );

		SendUDPMessage( ack.serialize(), endpoint );
	}
	
	private void SendMessage( ByteBuffer msg, InetSocketAddress endpoint )
	{
		UDPPacket data = new UDPPacket();
		data.initialize( EUdpPacketType.Data, thisSeq.incrementAndGet(), lastAckSeq.get() );
		data.getHeader().setDestConnID( remoteConnID );
		data.getHeader().setPacketsInMsg( 1 );
		data.getHeader().setMsgStartSeq( thisSeq.get() );
		
		data.setPayload( netfilter.processOutgoing( msg ) );
		
		SendUDPMessage( data.serialize(), endpoint );
	}
	
	public void SendAnonLogOn( Steam3Session session, CSteamID steamid )
	{
		ClientMsgProtobuf<MsgClientLogon> logon = new ClientMsgProtobuf<MsgClientLogon>(MsgClientLogon.class);
		
		logon.getHeader().setProtoHeader( CMsgProtoBufHeader.newBuilder()
				.setClientSteamId( new CSteamID(0, 0, session.getUniverse(), EAccountType.AnonGameServer).getLong() )
				.build() );
		
		logon.getMsg().setProto( CMsgClientLogon.newBuilder()
				.setObfustucatedPrivateIp( MsgClientLogon.ObfuscationMask )
				.setProtocolVersion( MsgClientLogon.CurrentProtocol )
				.setClientOsType( 10 )
				.build() );
		
		SendMessage( logon.serialize(), session.getEndpoint() );
	}
	
	public void SendConnect( Steam3Session session, int challenge )
	{
		UDPPacket connectreq = new UDPPacket();
		connectreq.initialize( EUdpPacketType.Connect, thisSeq.incrementAndGet(), lastAckSeq.get() );
		connectreq.getHeader().setDestConnID( remoteConnID );
		connectreq.getHeader().setPacketsInMsg( 1 );
		connectreq.getHeader().setMsgStartSeq( thisSeq.get() );
		
		ConnectData connectdata = new ConnectData();
		connectdata.setChallengeValue( challenge ^ ConnectData.CHALLENGE_MASK );
		
		connectreq.setPayload( connectdata.serialize() );
		
		SendUDPMessage( connectreq.serialize(), session.getEndpoint() );
	}
	
	public void SendHeartbeat( Steam3Session session )
	{
		ClientMsgProtobuf<MsgClientHeartBeat> heartbeat = new ClientMsgProtobuf<MsgClientHeartBeat>(MsgClientHeartBeat.class);
		session.FillProtoHdr( heartbeat );
		
		SendMessage( heartbeat.serialize(), session.getEndpoint() );
	}
	
	public void SendChallengeRequest( InetSocketAddress endpoint )
	{
		UDPPacket challengereq = new UDPPacket();
		challengereq.initialize( EUdpPacketType.ChallengeReq, 1, lastAckSeq.get() );

		SendUDPMessage( challengereq.serialize(), endpoint );
	}

	private void SendEncryptRequestResponse( MsgChannelEncryptRequest encreq, InetSocketAddress endpoint )
	{
		sessionKey = CryptoHelper.GenerateRandomBlock( 32 );
		
		UniverseKey unikey = UniverseKey.GetKey( encreq.getUniverse() );
		byte[] key = unikey.getKey();
		
		byte[] encryptedSession = CryptoHelper.RSAEncrypt( sessionKey, key );

		ClientMsg<MsgChannelEncryptResponse, MsgHdr> response = new ClientMsg<MsgChannelEncryptResponse, MsgHdr>( MsgChannelEncryptResponse.class, MsgHdr.class );
		
		Checksum checksum = new CRC32();
		checksum.update( encryptedSession, 0, encryptedSession.length );
		
		ByteBuffer payload = ByteBuffer.allocate( encryptedSession.length + 8 );
		payload.order( ByteOrder.LITTLE_ENDIAN );
		
		payload.put( encryptedSession );
		payload.putInt( (int)checksum.getValue() );
		payload.putInt( 0 );
		
		payload.flip();
		response.setPayload( payload );
		
		SendMessage( response.serialize(), endpoint );
	}
}
