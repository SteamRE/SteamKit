package net.steam3.filter;

import java.nio.ByteBuffer;

public interface ISteam3Filter
{
	public ByteBuffer processOutgoing( ByteBuffer input );
	public ByteBuffer processIncoming( ByteBuffer input );
}
