#ifndef STEAM_CRYPTO_H
#define STEAM_CRYPTO_H

void GenerateRandomBlock(unsigned char *pubRandomBlock, unsigned int cubRandomBlock);

bool RSAEncrypt(const unsigned char *pubPlaintextData, unsigned int cubPlaintextData, unsigned char *pubEncryptedData, unsigned int *pcubEncryptedData, const unsigned char *pubKey, unsigned int cubKey);

bool SymmetricEncrypt(const unsigned char *pubPlaintextData, unsigned int cubPlaintextData, unsigned char *pubEncryptedData, unsigned int *pcubEncryptedData, const unsigned char *pubKey, unsigned int cubKey);
bool SymmetricDecrypt(const unsigned char *pubEncryptedData, unsigned int cubEncryptedData, unsigned char *pubPlaintextData, unsigned int *pcubPlaintextData, const unsigned char *pubKey, unsigned int cubKey);

#endif //STEAM_CRYPTO_H