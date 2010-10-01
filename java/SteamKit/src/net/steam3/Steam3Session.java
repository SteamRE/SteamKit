package net.steam3;

import java.net.InetSocketAddress;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Future;
import java.util.concurrent.locks.ReentrantReadWriteLock;

import net.steam3.packets.ClientMsgProtobuf;
import steamkit.steam3.SteamMessages.CMsgProtoBufHeader;

import steamkit.steam3.SteamLanguage.EUniverse;
import steamkit.steam3.SteamLanguage.ExtendedClientMsgHdr;
import steamkit.types.CSteamID;

public class Steam3Session
{
	private ReentrantReadWriteLock lock;
	
	private Boolean connected;
	
	private InetSocketAddress endpoint;
	private EUniverse universe;
	private long steamID;
	private int sessionID;
	
	private Future<?> heartBeat;
	
	public Steam3Session()
	{
		lock = new ReentrantReadWriteLock();
		
		connected = false;
		endpoint = null;
		steamID = 0;
		universe = EUniverse.Invalid;
		sessionID = 0;
		heartBeat = null;
	}
	
	public void setHeartbeat( ExecutorService es, Runnable runner )
	{
		lock.writeLock().lock();
		heartBeat = es.submit( runner );
		lock.writeLock().unlock();
	}
	
	public void close()
	{
		lock.writeLock().lock();
		
		connected = false;
		
		if ( heartBeat != null )
			heartBeat.cancel( true );
		
		lock.writeLock().unlock();
	}
	
	public void setConnected( Boolean c )
	{
		lock.writeLock().lock();
		connected = c;
		lock.writeLock().unlock();
	}
	
	public void setEndpoint( InetSocketAddress endpoint )
	{
		lock.writeLock().lock();
		this.endpoint = endpoint;
		lock.writeLock().unlock();
	}
	
	public void setUniverse( EUniverse universe )
	{
		lock.writeLock().lock();
		this.universe = universe;
		lock.writeLock().unlock();
	}
	
	public void updateIDs( long steamID, int sessionID )
	{
		lock.writeLock().lock();
		
		this.steamID = steamID;
		this.sessionID = sessionID;
		
		lock.writeLock().unlock();
	}
	
	public Boolean getConnected()
	{
		lock.readLock().lock();
		Boolean ret = connected;
		lock.readLock().unlock();
		
		return ret;
	}
	
	public InetSocketAddress getEndpoint()
	{
		lock.readLock().lock();
		InetSocketAddress ret = endpoint;
		lock.readLock().unlock();
		
		return ret;
	}
	
	public EUniverse getUniverse()
	{
		lock.readLock().lock();
		EUniverse ret = universe;
		lock.readLock().unlock();
		
		return ret;
	}
	
	public CSteamID getSteamID()
	{
		lock.readLock().lock();
		CSteamID ret = new CSteamID(steamID);
		lock.readLock().unlock();
		
		return ret;
	}
	
	public void FillExtHdr( ExtendedClientMsgHdr header )
	{
		lock.readLock().lock();
		
		header.setSteamID( steamID );
		header.setSessionID( sessionID );
		
		lock.readLock().unlock();
	}
	
	public void FillProtoHdr( ClientMsgProtobuf<?> clientmsg )
	{
		lock.readLock().lock();
		
		clientmsg.getHeader().setProtoHeader( CMsgProtoBufHeader.newBuilder()
					.setClientSessionId( sessionID )
					.setClientSteamId( steamID )
					.build() );
	
		lock.readLock().unlock();
	}
	
}
