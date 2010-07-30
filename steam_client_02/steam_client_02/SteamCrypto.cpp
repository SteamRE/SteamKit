#include "SteamCrypto.h"

#include "cryptopp/aes.h"
#include "cryptopp/rsa.h"
#include "cryptopp/osrng.h"
#include "cryptopp/modes.h"

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
   
    RSAFunction params;
    params.BERDecode(source);

    RSA::PublicKey rsaPublicKey(params);
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

    AutoSeededRandomPool rnd;

    Rijndael::Encryption rijndaelEncryption;
    rijndaelEncryption.UncheckedSetKey(pubKey, cubKey, g_nullNameValuePairs);

    byte aesIV[16];
    GenerateRandomBlock(aesIV, AES::BLOCKSIZE);

    byte aesBlockIV[16];
    rijndaelEncryption.ProcessAndXorBlock(aesIV, NULL, aesBlockIV);

    CBC_Mode<AES>::Encryption aesEncryption;
    aesEncryption.SetCipherWithIV(rijndaelEncryption, aesIV);

    std::string encryptedDataStr;

    StreamTransformationFilter filter(aesEncryption, new StringSink(encryptedDataStr));
    filter.Put(pubPlaintextData, cubPlaintextData);
    filter.MessageEnd();

    *pcubEncryptedData = encryptedDataStr.length()+16;

    for (int i=0;i<16;i++)
        pubEncryptedData[i] = aesBlockIV[i];

    const char *encryptedData = encryptedDataStr.c_str();

    for (unsigned int i=0;i<*pcubEncryptedData;i++)
        pubEncryptedData[i+16] = encryptedData[i];

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

    Rijndael::Decryption rijndaelDecryption;
    rijndaelDecryption.UncheckedSetKey(pubKey, cubKey, g_nullNameValuePairs);

    byte aesIV[16];
    rijndaelDecryption.ProcessAndXorBlock(pubEncryptedData, NULL, aesIV);

    CBC_Mode<AES>::Decryption aesDecryption;
    aesDecryption.SetCipherWithIV(rijndaelDecryption, aesIV);

    std::string decryptedDataStr;

    StreamTransformationFilter filter(aesDecryption, new StringSink(decryptedDataStr));
    filter.Put(pubEncryptedData+16, cubEncryptedData-16);
    filter.MessageEnd();

    *pcubPlaintextData = decryptedDataStr.length();

    const char *decryptedData = decryptedDataStr.c_str();

    for (unsigned int i=0;i<*pcubPlaintextData;i++)
        pubPlaintextData[i] = decryptedData[i];

    return true;
}