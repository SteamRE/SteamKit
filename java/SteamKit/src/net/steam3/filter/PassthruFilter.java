package net.steam3.filter;

import java.nio.ByteBuffer;

public class PassthruFilter implements ISteam3Filter
{
	public PassthruFilter()
	{
	}
	
	public ByteBuffer processOutgoing( ByteBuffer input )
	{
		return input;
	}
	
	public ByteBuffer processIncoming( ByteBuffer input )
	{
		return input;
	}
}
