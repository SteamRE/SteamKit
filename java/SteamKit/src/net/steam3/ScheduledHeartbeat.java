package net.steam3;

import steamkit.util.Logger;

public class ScheduledHeartbeat implements Runnable
{
	private UDPConnection connection;
	
	private int heartbeatTime;
	private Steam3Session session;
	
	public ScheduledHeartbeat( UDPConnection connection, Steam3Session session, int heartbeat )
	{
		this.connection = connection;
		this.session = session;
		setHeartbeat( heartbeat );
	}
	
	public void setHeartbeat( int time )
	{
		this.heartbeatTime = time * 1000;
	}
	
	public void run()
	{
		try
		{
			for(;;)
			{
				connection.SendHeartbeat( session );
				Logger.getLogger().println( "Session heartbeat." );
				
				Thread.sleep( heartbeatTime );
			}
		} catch (InterruptedException e) {
			return;
		}
	}
}
