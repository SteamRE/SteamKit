package net.steam3;

import java.nio.ByteBuffer;

import steamkit.steam3.SteamLanguage.*;
import steamkit.util.MsgUtil;


public class Data
{
	private EMsg peekmsg;
	private ByteBuffer buffer;
	
	public Data()
	{
		peekmsg = EMsg.Invalid;
		buffer = null;
	}

	public EMsg peekType()
	{
		return peekmsg;
	}
	
	public ByteBuffer getBuffer()
	{
		return buffer;
	}
	
	public void deserialize( ByteBuffer buf )
	{
		buffer = buf;
		
		buf.mark();
		int emsg = buf.getInt();
		peekmsg = MsgUtil.GetMsg( emsg );
		buf.reset();
	}
}
