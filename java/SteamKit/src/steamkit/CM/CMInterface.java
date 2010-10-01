package steamkit.CM;

import net.steam3.Data;
import net.steam3.IUDPCallbacks;
import net.steam3.ScheduledHeartbeat;
import net.steam3.Steam3Session;
import net.steam3.UDPConnection;
import net.steam3.packets.ClientMsg;
import net.steam3.packets.ClientMsgProtobuf;
import steamkit.steam3.SteamLanguage.MsgClientLogonResponse;
import steamkit.steam3.SteamLanguage.MsgHdrProtoBuf;
import steamkit.steam3.SteamLanguage.*;

import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import com.google.protobuf.InvalidProtocolBufferException;

import steamkit.types.CSteamID;
import steamkit.util.Logger;

/*
 * Connection Master interface
 */
public class CMInterface implements IUDPCallbacks
{
	private static final String[] CMServers = new String[]
	{
        "68.142.64.164",
        "68.142.64.165",
        "68.142.91.34",
        "68.142.91.35",
        "68.142.91.36",
        "68.142.116.178",
        "68.142.116.179",

        "69.28.145.170",
        "69.28.145.171",
        "69.28.145.172",
        "69.28.156.250",

        "72.165.61.185",
        "72.165.61.186",
        "72.165.61.187",
        "72.165.61.188",

        "208.111.133.84",
        "208.111.133.85",
        "208.111.158.52",
        "208.111.158.53",
        "208.111.171.82",
        "208.111.171.83",
	};

	private static final ExecutorService es = Executors.newFixedThreadPool( 2 ); //Executors.newCachedThreadPool();
	
	private UDPConnection connection;
	
	private InetSocketAddress optimalEndpoint;
	private ChallengeData optimalChallenge;
	private CountDownLatch optimalCountdown;
	
	private CountDownLatch connectLatch;
	
	private Steam3Session session;
	
	public CMInterface()
	{
		session = new Steam3Session();
		connection = new UDPConnection( es, this );
	}
	
	public void close()
	{
		if ( session.getConnected() )
		{
			// send disconnect message..
		}
		
		session.close();
		connection.close();
	}
	
	public CountDownLatch initialize()
	{
		int optimalCount = Math.max( 1, (int) (CMServers.length / 1.5) );
		optimalCountdown = new CountDownLatch( optimalCount );
		
		for( String ip : CMServers )
		{
			connection.SendChallengeRequest( new InetSocketAddress( ip, 27017 ) );
		}
		
		return optimalCountdown;
	}
	
	public CountDownLatch connect()
	{
		connectLatch = new CountDownLatch( 1 );
		
		connection.SendConnect( session, optimalChallenge.getChallengeValue() );
		
		return connectLatch;
	}
	
	public Boolean isConnected()
	{
		return session.getConnected();
	}
	
	public void anonSignOn( EAccountType type )
	{
		CSteamID steamid = new CSteamID( 0, 0, session.getUniverse(), type );

		connection.SendAnonLogOn( session, steamid );
	}
	
	public void SignOnResponse( MsgHdrProtoBuf header, MsgClientLogonResponse response )
	{
		EResult result = EResult.lookup( response.getProto().getEresult() );
		
		Logger.getLogger().println( "Sign on response: " + result );
		
		if ( result == EResult.OK )
		{
			session.updateIDs( header.getProtoHeader().getClientSteamId(), header.getProtoHeader().getClientSessionId() );
			
			ScheduledHeartbeat heartbeat = new ScheduledHeartbeat( connection, session, response.getProto().getOutOfGameHeartbeatSeconds() );
			session.setHeartbeat( es, (Runnable) heartbeat );
			
			Logger.getLogger().println( "Signed on. ID " + session.getSteamID().getLong() );
		}
	}
	
	public void SessionNegotiationInitiated( EUniverse universe )
	{
		session.setUniverse( universe );
		
		Logger.getLogger().println( "Session negotiation started for universe " + universe );
	}
	
	public void SessionNegotiationCompleted( EResult result )
	{
		if ( result == EResult.OK )
		{
			Logger.getLogger().println( "Session negotiation complete." );
		}
		else
		{
			Logger.getLogger().println( "Session negotiation failed." );
		}
		
		connectLatch.countDown();
	}
	
	public void ChalllengeResponseCallback( ChallengeData challenge, InetSocketAddress remoteEndpoint )
	{
		if ( optimalCountdown == null || optimalCountdown.getCount() <= 0 )
			return;
	
		
		// needs sync
		if ( optimalChallenge == null || challenge.getServerLoad() < optimalChallenge.getServerLoad() )
		{
			optimalChallenge = challenge;
			optimalEndpoint = remoteEndpoint;
		}
	
		if ( optimalCountdown.getCount() <= 1 )
		{
			session.setEndpoint( optimalEndpoint );
		}
		
		optimalCountdown.countDown();
	}
	
	public void RecvDataCallback( Data data, InetSocketAddress remoteEndpoint )
	{
		ByteBuffer buf = data.getBuffer();
		
		Logger.getLogger().println( "Got EMsg: " + data.peekType() );
		
		switch( data.peekType() )
		{
		case ClientLogOnResponse:
			ClientMsgProtobuf<MsgClientLogonResponse> msg = null;
			try {
				msg = new ClientMsgProtobuf<MsgClientLogonResponse>(MsgClientLogonResponse.class, buf);
			} catch (InvalidProtocolBufferException e) {
				e.printStackTrace();
			}
			
			SignOnResponse( msg.getHeader(), msg.getMsg() );
			break;
		}
	}
	
	public void AcceptCallback( Accept accept )
	{
		Logger.getLogger().println( "Connection accepted." );
	}
	
	public void DisconnectCallback( Disconnect disconnect )
	{
		Logger.getLogger().println( "Connection closed." );
		close();
		
		connectLatch.countDown();
	}
}
