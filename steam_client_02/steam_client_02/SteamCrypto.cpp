#include "SteamCrypto.h"

#include "cryptopp/aes.h"
#include "cryptopp/rsa.h"
#include "cryptopp/osrng.h"
#include "cryptopp/modes.h"
#include "cryptopp/sha.h"
#include "cryptopp/filters.h"

using namespace CryptoPP;

void GenerateRandomBlock(unsigned char *pubRandomBlock, unsigned int cubRandomBlock)
{
	AutoSeededRandomPool rnd; 
	rnd.GenerateBlock(pubRandomBlock, cubRandomBlock);
}

bool RSAEncrypt(const unsigned char *pubPlaintextData, unsigned int cubPlaintextData, unsigned char *pubEncryptedData, unsigned int *pcubEncryptedData, const unsigned char *pubKey, unsigned int cubKey)
{
	if (!pubPlaintextData)
		return false;

	if (!cubPlaintextData)
		return false;

	if (!pubEncryptedData)
		return false;

	if (!pcubEncryptedData)
		return false;

	if (!pubKey)
		return false;

	if (!cubKey)
		return false;

	AutoSeededRandomPool rnd;

	StringSource source(pubKey, cubKey, true);

	RSA::PublicKey rsaPublicKey;
	
	try
	{
		rsaPublicKey.BERDecode( source );
	}
	catch(...)
	{
		printf("RSAEncrypt: Unable to decode public key\n");

		return false;
	}
	
	RSAES_OAEP_SHA_Encryptor rsaEncryption(rsaPublicKey);

	std::string encryptedDataStr;

	StringSource(pubPlaintextData, cubPlaintextData, true,
		new PK_EncryptorFilter(rnd, rsaEncryption,
			new StringSink(encryptedDataStr)
		)
	);

	*pcubEncryptedData = encryptedDataStr.length()+16;

	const char *encryptedData = encryptedDataStr.c_str();

	for (unsigned int i=0;i<*pcubEncryptedData;i++)
		pubEncryptedData[i] = encryptedData[i];

	return true;
}

bool AESEncrypt(const unsigned char *pubPlaintextData, unsigned int cubPlaintextData, unsigned char *pubEncryptedData, unsigned int *pcubEncryptedData, const unsigned char *pubKey, unsigned int cubKey, const unsigned char *pubIV, unsigned int cubIV)
{
	if (!pubPlaintextData)
		return false;

	if (!cubPlaintextData)
		return false;

	if (!pubEncryptedData)
		return false;

	if (!pubKey)
		return false;

	if (!cubKey)
		return false;

	if (!pubIV)
		return false;

	if (!cubIV)
		return false;

	assert( cubIV == AES::BLOCKSIZE );
	
	CBC_Mode<AES>::Encryption aesEncryption( pubKey, cubKey, pubIV);

	std::string strEncryptedData;

	/*Array*/ StringSource( pubPlaintextData, cubPlaintextData, true,
		new StreamTransformationFilter( aesEncryption,
			new StringSink( strEncryptedData )
		)
	);

	unsigned int nLength = strEncryptedData.length();

	if (pcubEncryptedData)
		*pcubEncryptedData = nLength;

	memcpy(pubEncryptedData, strEncryptedData.c_str(), nLength);
	
	return true;
}

bool AESDecrypt(const unsigned char *pubEncryptedData, unsigned int cubEncryptedData, unsigned char *pubPlaintextData, unsigned int *pcubPlaintextData, const unsigned char *pubKey, unsigned int cubKey, const unsigned char *pubIV, unsigned int cubIV)
{
    if (!pubEncryptedData)
        return false;

    if (!cubEncryptedData)
        return false;

    if (!pubPlaintextData)
        return false;

    if (!pubKey)
        return false;

    if (!cubKey)
        return false;

	if (!pubIV)
		return false;

	if (!cubIV)
		return false;

	assert( cubIV == AES::BLOCKSIZE );
	
    CBC_Mode<AES>::Decryption aesDecryption( pubKey, cubKey, pubIV);

    std::string strDecryptedData;

	/*Array*/ StringSource( pubEncryptedData, cubEncryptedData, true,
		new StreamTransformationFilter( aesDecryption,
			new StringSink( strDecryptedData )
		)
	);

	unsigned int nLength = strDecryptedData.length();

	if (pcubPlaintextData)
		*pcubPlaintextData = nLength;

	memcpy(pubPlaintextData, strDecryptedData.c_str(), nLength);

    return true;
}

bool SymmetricEncrypt(const unsigned char *pubPlaintextData, unsigned int cubPlaintextData, unsigned char *pubEncryptedData, unsigned int *pcubEncryptedData, const unsigned char *pubKey, unsigned int cubKey)
{
	if (!pubPlaintextData)
		return false;

	if (!cubPlaintextData)
		return false;

	if (!pubEncryptedData)
		return false;

	if (!pcubEncryptedData)
		return false;

	if (!pubKey)
		return false;

	if (!cubKey)
		return false;

	Rijndael::Encryption rijndaelEncryption( pubKey, cubKey );
	ECB_Mode_ExternalCipher::Encryption ECBEncryption( rijndaelEncryption );

	byte aesIV[16];
	byte aesBlockIV[16];
	GenerateRandomBlock(aesIV, AES::BLOCKSIZE);
	
	/*Array*/ StringSource( aesIV, sizeof(aesIV), true, 
		new StreamTransformationFilter( ECBEncryption,
			new ArraySink( aesBlockIV, sizeof(aesBlockIV) )
		) 
	);

	CBC_Mode_ExternalCipher::Encryption aesEncryption( rijndaelEncryption, aesIV );

	std::string strEncryptedData;

	/*Array*/ StringSource( pubPlaintextData, cubPlaintextData, true,
		new StreamTransformationFilter( aesEncryption,
			new StringSink( strEncryptedData )
		)
	);

	*pcubEncryptedData = strEncryptedData.length() + 16;

	for (int i=0;i<16;i++)
		pubEncryptedData[i] = aesBlockIV[i];

	const char *pchEncryptedData = strEncryptedData.c_str();

	for (unsigned int i=0;i<*pcubEncryptedData;i++)
		pubEncryptedData[i+16] = pchEncryptedData[i];

	return true;
}

bool SymmetricDecrypt(const unsigned char *pubEncryptedData, unsigned int cubEncryptedData, unsigned char *pubPlaintextData, unsigned int *pcubPlaintextData, const unsigned char *pubKey, unsigned int cubKey)
{
	if (!pubEncryptedData)
		return false;

	if (!cubEncryptedData)
		return false;

	if (!pubPlaintextData)
		return false;

	if (!pcubPlaintextData)
		return false;

	if (!pubKey)
		return false;

	if (!cubKey)
		return false;

	if (cubEncryptedData <= 16)
		return false;

	Rijndael::Decryption rijndaelEncryption( pubKey, cubKey );
	ECB_Mode_ExternalCipher::Decryption ECBDecryption( rijndaelEncryption );

	byte aesIV[16];

	/*Array*/ StringSource( pubEncryptedData, sizeof(aesIV), true, 
		new StreamTransformationFilter( ECBDecryption,
			new ArraySink( aesIV, sizeof(aesIV) )
		) 
	);

	CBC_Mode_ExternalCipher::Decryption aesDecryption( rijndaelEncryption, aesIV );

	std::string strDecryptedData;

	/*Array*/ StringSource( pubEncryptedData + 16, cubEncryptedData - 16, true,
		new StreamTransformationFilter( aesDecryption,
			new StringSink( strDecryptedData )
		)
	);

	*pcubPlaintextData = strDecryptedData.length();

	const char *pchDecryptedData = strDecryptedData.c_str();

	for (unsigned int i=0;i<*pcubPlaintextData;i++)
		pubPlaintextData[i] = pchDecryptedData[i];

	return true;
}

bool SHA1Hash(const unsigned char *pubData, unsigned int cubData, unsigned char *pubHash)
{
	if (!pubData)
		return false;

	if (!cubData)
		return false;

	if (!pubHash)
		return false;

	SHA1 sha1Hash;
	sha1Hash.CalculateDigest(pubHash, pubData, cubData);

	return true;
}