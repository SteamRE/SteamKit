package net.steam3.filter;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

import steamkit.util.CryptoHelper;

public class EncryptionFilter implements ISteam3Filter
{
	private byte[] sessionKey;
	
	public EncryptionFilter( byte[] key )
	{
		sessionKey = key;
	}
	
	public ByteBuffer processOutgoing( ByteBuffer input )
	{
		byte[] outgoing = CryptoHelper.SymmetricEncrypt( input.array(), sessionKey );
		ByteBuffer outgoingbuf = ByteBuffer.wrap( outgoing );
		outgoingbuf.order( ByteOrder.LITTLE_ENDIAN );
		
		return outgoingbuf;
	}
	
	public ByteBuffer processIncoming( ByteBuffer input )
	{
		byte[] incoming = CryptoHelper.SymmetricDecrypt( input.array() , sessionKey );
		ByteBuffer incomingbuf = ByteBuffer.wrap( incoming );
		incomingbuf.order( ByteOrder.LITTLE_ENDIAN );
		
		return incomingbuf;
	}
}
