package steamkit.CM;

import net.steam3.IUDPCallbacks;
import net.steam3.UDPConnection;
import net.steam3.packets.*;
import net.steam3.packets.msg.ClientLogOnResponse;
import net.steam3.packets.udp.Accept;
import net.steam3.packets.udp.Challenge;
import net.steam3.packets.udp.Data;
import net.steam3.packets.udp.Disconnect;

import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.util.concurrent.CountDownLatch;

import steamkit.types.CSteamID;
import steamkit.types.EAccountType;
import steamkit.types.EResult;
import steamkit.types.EUniverse;

/*
 * Connection Master interface
 */
public class CMInterface implements IUDPCallbacks
{
	private static final String[] CMServers = new String[]
	{
       /* "68.142.64.164",
        "68.142.64.165",
        "68.142.91.34",
        "68.142.91.35",
        "68.142.91.36",
        "68.142.116.178",
        "68.142.116.179",

        "69.28.145.170",
        "69.28.145.171",
        "69.28.145.172",*/
        "69.28.156.250",
/*
        "72.165.61.185",
        "72.165.61.186",
        "72.165.61.187",
        "72.165.61.188",

        "208.111.133.84",
        "208.111.133.85",
        "208.111.158.52",
        "208.111.158.53",
        "208.111.171.82",
        "208.111.171.83",*/
	};

	private UDPConnection connection;
	
	private InetSocketAddress optimalEndpoint;
	private Challenge optimalChallenge;
	private CountDownLatch optimalCountdown;
	
	private CountDownLatch connectLatch;
	
	private Boolean connected;
	private EUniverse connectedUniverse;
	
	public CMInterface()
	{
		connected = false;
		connection = new UDPConnection( this );
	}
	
	public void close()
	{
		connection.close();
		connected = false;
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
		
		connection.SendConnect( optimalChallenge.getChallenge(), optimalEndpoint );
		
		return connectLatch;
	}
	
	public Boolean isConnected()
	{
		return connected;
	}
	
	public void anonSignOn( EAccountType type )
	{
		CSteamID steamid = new CSteamID( 0, 0, connectedUniverse, type );

		connection.SendAnonLogOn( steamid, optimalEndpoint );
	}
	
	public void SignOnResponse( EResult result )
	{
		System.out.println( "Sign on response: " + result );
	}
	
	public void SessionNegotiationInitiated( EUniverse universe )
	{
		connectedUniverse = universe;
		
		System.out.println( "Session negotiation started for universe " + universe );
	}
	
	public void SessionNegotiationCompleted( EResult result )
	{
		if ( result == EResult.OK )
		{
			System.out.println( "Session negotiation complete." );
		}
		else
		{
			System.out.println( "Session negotiation failed." );
		}
		
		connectLatch.countDown();
	}
	
	public void ChalllengeResponseCallback( Challenge challenge, InetSocketAddress remoteEndpoint )
	{
		if ( optimalCountdown == null || optimalCountdown.getCount() <= 0 )
			return;
	
		if ( optimalChallenge == null || challenge.getLoad() < optimalChallenge.getLoad() )
		{
			optimalChallenge = challenge;
			optimalEndpoint = remoteEndpoint;
		}
	
		optimalCountdown.countDown();
	}
	
	public void RecvDataCallback( Data data, InetSocketAddress remoteEndpoint )
	{
		ByteBuffer buf = data.getBuffer();
		
		System.out.println( "Got EMsg: " + data.peekType() );
		
		switch( data.peekType() )
		{
		case ClientLogOnResponse:
			ExtendedClientMsgHdr.deserialize( buf );
			ClientLogOnResponse response = ClientLogOnResponse.deserialize( buf );
			
			SignOnResponse( response.getResult() );
			break;
		}
	}
	
	public void AcceptCallback( Accept accept )
	{
		System.out.println( "Connection accepted." );
	}
	public void DisconnectCallback( Disconnect disconnect )
	{
		System.out.println( "Connection closed." );
		close();
		
		connectLatch.countDown();
	}
}
