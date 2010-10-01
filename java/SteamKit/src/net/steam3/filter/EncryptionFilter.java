package net.steam3.filter;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import steamkit.util.CryptoHelper;

public class EncryptionFilter implements ISteam3Filter
{
	private byte[] sessionKey;
	
	private Boolean activated;
	
	public EncryptionFilter( byte[] key )
	{
		activated = false;
		sessionKey = key;
	}
	
	public ByteBuffer processOutgoing( ByteBuffer input )
	{
		activated = true;
		
		byte[] outgoing = CryptoHelper.SymmetricEncrypt( input.array(), sessionKey );
		ByteBuffer outgoingbuf = ByteBuffer.wrap( outgoing );
		outgoingbuf.order( ByteOrder.LITTLE_ENDIAN );
		
		return outgoingbuf;
	}
	
	public ByteBuffer processIncoming( ByteBuffer input )
	{
		// only activate incoming encryption once we've sent a message (the server will send unecrypted until it gets a reply, useful if we're stepping)
		if ( !activated )
		{
			return input;
		}
		
		byte[] incoming = CryptoHelper.SymmetricDecrypt( input.array() , sessionKey );
		ByteBuffer incomingbuf = ByteBuffer.wrap( incoming );
		incomingbuf.order( ByteOrder.LITTLE_ENDIAN );
		
		return incomingbuf;
	}
}
