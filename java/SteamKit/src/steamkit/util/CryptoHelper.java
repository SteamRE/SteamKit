/**
 * SteamKit CryptoHelper
 */
package steamkit.util;

import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

import org.bouncycastle.jce.provider.BouncyCastleProvider;

import java.security.KeyFactory;
import java.security.MessageDigest;
import java.security.SecureRandom;
import java.security.Security;
import java.security.interfaces.RSAPublicKey;
import java.security.spec.X509EncodedKeySpec;

/**
 * Steam crypto methods
 */
public class CryptoHelper
{
	static
	{
		Security.addProvider( new BouncyCastleProvider() );
	}
	
	private static final String RSAFORM = "RSA/ECB/OAEPWithSHA1AndMGF1Padding";
	private static final String AESFORM = "AES/CBC/PKCS7Padding";
	private static final String AESIVFORM = "AES/ECB/NoPadding";
	
	/**
	 * Encrypt "plaintext" using X509 encoded pubkey "key"
	 * @note verified
	 */
	public static byte[] RSAEncrypt( byte[] plaintext, byte[] key )
	{
		try 
		{
			X509EncodedKeySpec keySpec = new X509EncodedKeySpec( key );
			
			KeyFactory keyFactory = KeyFactory.getInstance( "RSA" );
			RSAPublicKey pubKey = (RSAPublicKey)keyFactory.generatePublic( keySpec );

			Cipher rsaCipher = Cipher.getInstance( RSAFORM, "BC" );
			rsaCipher.init( Cipher.ENCRYPT_MODE, pubKey );
			
			return rsaCipher.doFinal( plaintext );
		}
		catch( Exception e )
		{
			e.printStackTrace();
		}
		
		return null;
	}

	/**
	 * Generate SHA digest of buffer
	 * @note verified
	 */
	public static byte[] SHADigest( byte[] plaintext )
	{
		try
		{
			MessageDigest shaDigest = MessageDigest.getInstance( "SHA" );
			
			return shaDigest.digest( plaintext );
		}
		catch( Exception e )
		{
			e.printStackTrace();
		}
		
		return null;
	}
	
	/**
	 * Encrypt "plaintext" with aes key "key" and iv "iv"
	 */
	public static byte[] AESEncrypt( byte[] plaintext, byte[] key, byte[] iv )
	{
		try
		{
			SecretKeySpec aesKey = new SecretKeySpec( key, "AES" );
			IvParameterSpec aesIV = new IvParameterSpec( iv );
			
			Cipher aesCipher = Cipher.getInstance( AESFORM, "BC" );
			aesCipher.init( Cipher.ENCRYPT_MODE, aesKey, aesIV );

			return aesCipher.doFinal( plaintext );
		}
		catch( Exception e )
		{
			e.printStackTrace();
		}
		
		return null;
	}
	
	/**
	 * Decrypt "ciphertext" with aes key "key" and iv "iv"
	 */
	public static byte[] AESDecrypt( byte[] ciphertext, byte[] key, byte[] iv )
	{
		try
		{
			SecretKeySpec aesKey = new SecretKeySpec( key, "AES" );
			IvParameterSpec aesIV = new IvParameterSpec( iv );
			
			Cipher aesCipher = Cipher.getInstance( AESFORM, "BC" );
			aesCipher.init( Cipher.DECRYPT_MODE, aesKey, aesIV );

			return aesCipher.doFinal( ciphertext );
		}
		catch( Exception e )
		{
			e.printStackTrace();
		}
		
		return null;
	}

	/**
	 * Symmetric encrypt "plaintext" with aes key "key" and a random (encrypted) IV
	 * @note verified
	 */
	public static byte[] SymmetricEncrypt( byte[] plaintext, byte[] key )
	{
		try
		{
			SecretKeySpec aesKey = new SecretKeySpec( key, "AES" );
			
			Cipher aesCipher = Cipher.getInstance( AESIVFORM, "BC" );
			aesCipher.init( Cipher.ENCRYPT_MODE, aesKey );
			
			byte[] plainIV = GenerateRandomBlock( 16 );
			byte[] cryptedIV = aesCipher.doFinal( plainIV );
			
			IvParameterSpec aesIV = new IvParameterSpec( plainIV );
			
			aesCipher = Cipher.getInstance( AESFORM, "BC" );
			aesCipher.init( Cipher.ENCRYPT_MODE, aesKey, aesIV );

			byte[] ciphertext = aesCipher.doFinal( plaintext );
			
			byte[] finalbuf = new byte[ciphertext.length + cryptedIV.length];
			
			System.arraycopy( cryptedIV, 0, finalbuf, 0, cryptedIV.length );
			System.arraycopy( ciphertext, 0, finalbuf, cryptedIV.length, ciphertext.length );
			
			return finalbuf;
		}
		catch( Exception e)
		{
			e.printStackTrace();
		}
		
		return null;
	}
	
	/**
	 * Symmetric decrypt "ciphertext" with aes key "key" (using IV from ciphertext)
	 * @note verified
	 */
	public static byte[] SymmetricDecrypt( byte[] ciphertext, byte[] key )
	{
		try
		{
			SecretKeySpec aesKey = new SecretKeySpec( key, "AES" );
			
			Cipher aesCipher = Cipher.getInstance( AESIVFORM, "BC" );
			aesCipher.init( Cipher.DECRYPT_MODE, aesKey );
			
			byte[] plainIV = aesCipher.doFinal( ciphertext, 0, 16 );
			
			IvParameterSpec aesIV = new IvParameterSpec( plainIV );
			
			aesCipher = Cipher.getInstance( AESFORM, "BC" );
			aesCipher.init( Cipher.DECRYPT_MODE, aesKey, aesIV );
			
			return aesCipher.doFinal( ciphertext, plainIV.length, ciphertext.length - plainIV.length );
		}
		catch( Exception e)
		{
			e.printStackTrace();
		}
		
		return null;
	}
	
	/**
	 * Generates a random block of size "size"
	 * @note verified
	 */
	public static byte[] GenerateRandomBlock( int size )
	{
		SecureRandom random = new SecureRandom();
		
		byte[] buffer = new byte[size];
		random.nextBytes( buffer );
		
		return buffer;
	}
	
}
