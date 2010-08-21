#define DEFAULT_PORT 50000

#define UNIX_EPOCH 0xDCBFFEFF2BC000

#include "NumberUtil.h"
#include "StringUtil.h"
#include "ByteBuffer.h"
#include "SteamCrypto.h"
#include "SteamSock.h"

#include "steam/emsg.h"

#include "tier1/checksum_crc.h"

#define STEAM
#include "steam/CSteamID.h"
#include "steam/SteamTypes.h"
#include "steam/clientmsgs.h"

#include <stdio.h>
#include <conio.h>
#include <ctime>
#include <sys/utime.h>

bool bChannelEncryption = false;

CSteamID clientSteamID;

struct TGTServerReadable
{
	uint32 m_unTicketSize;
	unsigned char m_rgchTicket[2048];
} tgtServerReadable;

unsigned char hardcodedUnknownMessageObject[163] =
{
    0x01, 0x42, 0x42, 0x33, 0x00, 0x33, 0x62, 0x66, 0x62, 0x64, 0x33, 0x61, 0x39, 0x36, 0x31, 0x65, 
    0x38, 0x62, 0x30, 0x63, 0x35, 0x36, 0x33, 0x63, 0x34, 0x38, 0x31, 0x30, 0x33, 0x65, 0x34, 0x65, 
    0x62, 0x30, 0x64, 0x63, 0x37, 0x66, 0x39, 0x33, 0x64, 0x35, 0x63, 0x65, 0x61, 0x00, 0x01, 0x46, 
    0x46, 0x32, 0x00, 0x61, 0x39, 0x64, 0x31, 0x39, 0x63, 0x34, 0x65, 0x37, 0x62, 0x33, 0x34, 0x35, 
    0x66, 0x38, 0x35, 0x64, 0x66, 0x66, 0x66, 0x39, 0x30, 0x61, 0x65, 0x35, 0x65, 0x66, 0x39, 0x38, 
    0x38, 0x38, 0x35, 0x31, 0x31, 0x62, 0x37, 0x37, 0x30, 0x30, 0x39, 0x00, 0x01, 0x33, 0x42, 0x33, 
    0x00, 0x33, 0x36, 0x66, 0x38, 0x38, 0x31, 0x66, 0x66, 0x31, 0x61, 0x35, 0x61, 0x36, 0x65, 0x38, 
    0x37, 0x64, 0x34, 0x62, 0x66, 0x31, 0x62, 0x64, 0x65, 0x30, 0x63, 0x36, 0x61, 0x62, 0x61, 0x31, 
    0x62, 0x66, 0x31, 0x65, 0x39, 0x38, 0x64, 0x31, 0x65, 0x00, 0x08, 0x08, 0x01, 0x00, 0x00, 0x00, 
    0x16, 0x05, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 
    0x00, 0x00, 0x00,
};

unsigned int hardcodedAuthTicketSz = 0xB0;
unsigned char hardcodedAuthTicket[0xB0] =
{
    0x30, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x43, 0x79, 0x21, 0x03, 0x01, 0x00, 0x10, 0x01,
    0x07, 0x00, 0x00, 0x00, 0xE8, 0x37, 0x03, 0x56, 0x09, 0x01, 0xA8, 0xC0, 0x00, 0x00, 0x00, 0x00,
    0xF8, 0x84, 0x6A, 0x4C, 0x78, 0x34, 0x86, 0x4C, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0xAA, 0x75, 0x0E, 0x97, 0x03, 0xA0, 0x5C, 0xC3, 0x58, 0x5F, 0xCF, 0xEB, 0xD6, 0x41, 0x09, 0x6A,
    0x39, 0xE5, 0x55, 0xA7, 0x38, 0xC7, 0x17, 0x47, 0xF3, 0x06, 0x8A, 0x0C, 0x09, 0x97, 0x03, 0x5C,
    0xE8, 0x91, 0x08, 0x94, 0x16, 0x82, 0xA5, 0xDC, 0xA8, 0x76, 0x3D, 0x15, 0xEF, 0x00, 0xCD, 0x86,
    0xFE, 0xB0, 0x94, 0x56, 0x06, 0xA8, 0xF3, 0x6A, 0x27, 0xDB, 0x62, 0xE0, 0x82, 0xDD, 0x92, 0x4B,
    0xCB, 0xE6, 0xC0, 0x58, 0xAB, 0x1C, 0xAF, 0x7A, 0xB6, 0xA0, 0x44, 0x71, 0x03, 0x31, 0xF6, 0xE7,
    0xB5, 0x05, 0xA4, 0x28, 0xA7, 0xAF, 0x86, 0xD8, 0xEB, 0x1A, 0xD4, 0x02, 0x11, 0xFE, 0x5C, 0x0D,
    0x51, 0x75, 0x9C, 0x97, 0x15, 0x89, 0xB0, 0x50, 0x10, 0x87, 0x41, 0x5A, 0x9C, 0x79, 0xAF, 0x39,
    0x20, 0xBD, 0xEB, 0x60, 0x9F, 0x49, 0x32, 0x13, 0xF3, 0xBD, 0x99, 0xFD, 0xBA, 0xA0, 0x16, 0xF5,
};

char chClientUsername[256];
char chClientPassword[256];

unsigned int nClientExternalIP;
unsigned int nClientInternalIP;

unsigned char aesSessionKeySteam2[32];
unsigned char aesSessionKeySteam3[32];

SteamNetManager netManager;

unsigned char k_rgchPublicKey_Public[160] = {
	0x30, 0x81, 0x9D, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x81, 0x8B, 0x00, 0x30, 0x81, 0x87, 0x02, 0x81, 0x81, 0x00, 0xDF, 0xEC, 0x1A,
	0xD6, 0x2C, 0x10, 0x66, 0x2C, 0x17, 0x35, 0x3A, 0x14, 0xB0, 0x7C, 0x59, 0x11, 0x7F, 0x9D, 0xD3, 0xD8, 0x2B, 0x7A, 0xE3, 0xE0, 0x15, 0xCD, 0x19, 0x1E, 0x46, 0xE8, 0x7B, 0x87, 0x74, 0xA2, 0x18,
	0x46, 0x31, 0xA9, 0x03, 0x14, 0x79, 0x82, 0x8E, 0xE9, 0x45, 0xA2, 0x49, 0x12, 0xA9, 0x23, 0x68, 0x73, 0x89, 0xCF, 0x69, 0xA1, 0xB1, 0x61, 0x46, 0xBD, 0xC1, 0xBE, 0xBF, 0xD6, 0x01, 0x1B, 0xD8,
	0x81, 0xD4, 0xDC, 0x90, 0xFB, 0xFE, 0x4F, 0x52, 0x73, 0x66, 0xCB, 0x95, 0x70, 0xD7, 0xC5, 0x8E, 0xBA, 0x1C, 0x7A, 0x33, 0x75, 0xA1, 0x62, 0x34, 0x46, 0xBB, 0x60, 0xB7, 0x80, 0x68, 0xFA, 0x13,
	0xA7, 0x7A, 0x8A, 0x37, 0x4B, 0x9E, 0xC6, 0xF4, 0x5D, 0x5F, 0x3A, 0x99, 0xF9, 0x9E, 0xC4, 0x3A, 0xE9, 0x63, 0xA2, 0xBB, 0x88, 0x19, 0x28, 0xE0, 0xE7, 0x14, 0xC0, 0x42, 0x89, 0x02, 0x01, 0x11,
};


struct UDPPktTypeChallenge_t
{
	uint32 m_nChallenge;
	uint32 m_nUnknown;
};

struct UDPPktTypeConnect_t
{
	uint32 m_nObfuscatedChallenge;
};


bool UTIL_GetLocalAddress(unsigned long *addr)
{
	char name[256];
			
	if (gethostname(name, 256) == SOCKET_ERROR)
		return false;

	addrinfo *info;

	if (getaddrinfo(name, NULL, NULL, &info) != 0)
		return false;

	for (addrinfo *p=info;p != NULL;p = p->ai_next)
	{
		if (p->ai_family == AF_INET)
		{
			*addr = ((sockaddr_in *)p->ai_addr)->sin_addr.s_addr;
			
			return true;
		}
	}

	return false;
}

bool CMD_GetKeyValue(int argc, char **argv[], const char *key, char *value)
{
	if (argc <= 0)
		return false;

	for (int i=0;i<argc;i++)
	{
		if (!strcmp((const char *)argv[i], key))
		{
			if (argc > i+1)
			{
				strcpy(value, (const char *)argv[i+1]);

				return true;
			}
		}
	}

	return false;
}

int main(int argc, char **argv[])
{
	if (!CMD_GetKeyValue(argc, argv, "-username", chClientUsername))
	{
		printf("You must provide a username in the command line (-username xxx)\n");
		_getch();

		return 0;
	}

	if (!CMD_GetKeyValue(argc, argv, "-password", chClientPassword))
	{
		printf("You must provide a password in the command line (-password xxx)\n");
		_getch();

		return 0;
	}

	netManager.Initialize();

	char cmdIP[16];
	unsigned long sckIP = 0;

	if (CMD_GetKeyValue(argc, argv, "-ip", cmdIP))
		sckIP = inet_addr(cmdIP);
	else if (!UTIL_GetLocalAddress(&sckIP))
		return netManager.ShutdownWithMessage("Fatal error, failed to get local address");

	char cmdPort[8];
	unsigned short sckPort = 0;

	if (CMD_GetKeyValue(argc, argv, "-port", cmdPort))
		sckPort = htons(atoi(cmdPort));
	else
		sckPort = htons(DEFAULT_PORT);

	sockaddr_in local;
	local.sin_addr.s_addr = sckIP;
	local.sin_port = sckPort;

	SteamAddr saLocal(local);
	SteamAddr saTargetTCP("72.165.61.138", 27039);
	SteamAddr saTargetUDP("68.142.64.165", 27017);
	//SteamAddr saTargetUDP("69.28.145.170", 27017);

	bool bTrySteam3 = false;

	SteamSockTCP skTCP(saLocal, saTargetTCP);

	if (skTCP.IsBound())
	{
		printf("TCP socket initialized (%s)\n", saLocal.ToString());
		printf("Initiating connection with %s\n", saTargetTCP.ToString());

		char requestExternalIPData[512];
		BBWrite requestExternalIPBuffer(requestExternalIPData, 512);
		requestExternalIPBuffer.WriteNumber32BE(0);
		requestExternalIPBuffer.WriteByte(4);
		requestExternalIPBuffer.WriteNumber32BE((nClientInternalIP = sckIP));
		requestExternalIPBuffer.WriteNumber32BE(1); //Protocol version?

		if (skTCP.Send(requestExternalIPBuffer.GetData(), requestExternalIPBuffer.GetBufferPosition()))
		{
			skTCP.SetExpectedPacket(k_ETCPPktTypeExternalIP);

			while (true)
			{
				char chData[2048];
				unsigned int cchData = 0;

				if (skTCP.Recv(chData, 2048, &cchData))
				{
					BBRead chDataBuffer(chData, cchData);

					printf("Received TCP packet!\n");

					ETCPPktType expectedPacket = skTCP.GetExpectedPacket();

					if (expectedPacket == k_ETCPPktTypeExternalIP)
					{
						printf("k_ETCPPktTypeExternalIP\n");

						unsigned char cUnknown1 = chDataBuffer.ReadByte();

						if (cUnknown1 == 0)
						{
							IN_ADDR extTest;
							extTest.s_addr = (nClientExternalIP = chDataBuffer.ReadNumber32BE());

							printf("External IP: %s\n", inet_ntoa(extTest));

							unsigned int cbClientUsername = StringLength(chClientUsername);

							char credentialsData[512];
							BBWrite credentialsBuffer(credentialsData, 512);
							credentialsBuffer.WriteNumber32BE((cbClientUsername*2)+5); //Packet size, excluding this - const for now
							credentialsBuffer.WriteByte(2); //Command? AuthenticateAndRequestTGT
							//Data
							credentialsBuffer.WriteNumber16BE(cbClientUsername);
							credentialsBuffer.WriteBytes(strlwr(chClientUsername), cbClientUsername);
							credentialsBuffer.WriteNumber16BE(cbClientUsername);
							credentialsBuffer.WriteBytes(strlwr(chClientUsername), cbClientUsername);

							if (skTCP.Send(credentialsBuffer.GetData(), credentialsBuffer.GetBufferPosition()))
								skTCP.SetExpectedPacket(k_ETCPPktTypePostCredentials);
						}
					}
					else if (expectedPacket == k_ETCPPktTypePostCredentials)
					{
						printf("Post credentials data!\n");

						unsigned int nSalt1 = chDataBuffer.ReadNumber32LE();
						unsigned int nSalt2 = chDataBuffer.ReadNumber32LE();

						char saltedPasswordData[512];
						BBWrite saltedPasswordBuffer(saltedPasswordData, 512);
						saltedPasswordBuffer.WriteNumber32LE(nSalt1);
						saltedPasswordBuffer.WriteBytes(chClientPassword, StringLength(chClientPassword));
						saltedPasswordBuffer.WriteNumber32LE(nSalt2);

						SHA1Hash((const unsigned char *)saltedPasswordBuffer.GetData(), saltedPasswordBuffer.GetBufferPosition(), aesSessionKeySteam2);

						char hashedIPData[32];
						BBWrite hashedIPBuffer(hashedIPData, 32);
						hashedIPBuffer.WriteNumber32LE(nClientExternalIP);
						hashedIPBuffer.WriteNumber32LE(nClientInternalIP);

						for (int i=0;i<hashedIPBuffer.GetBufferPosition();i++)
							printf("%02X ", (unsigned char)hashedIPBuffer.GetData()[i]);
						printf("\n\n");

						unsigned char timeObfuscationData[20];
						SHA1Hash((const unsigned char *)hashedIPBuffer.GetData(), hashedIPBuffer.GetBufferPosition(), timeObfuscationData);

						unsigned long long ulTime = UNIX_EPOCH+(time(NULL)*1000000);
						unsigned long long ulTimeObfuscation = *(unsigned long long *)timeObfuscationData;
						unsigned long long ulTimeObfuscated = ulTime ^ ulTimeObfuscation;

						char plaintextAuthData[12];
						BBWrite plaintextAuthBuffer(plaintextAuthData, 12);
						plaintextAuthBuffer.WriteNumber64LE(ulTimeObfuscated);
						plaintextAuthBuffer.WriteNumber32LE(nClientInternalIP);

						byte aesIV[16];
						GenerateRandomBlock(aesIV, 16);
						
						unsigned char aesCipher[16];
						unsigned int aesCipherSz = 0;

						AESEncrypt((const unsigned char *)plaintextAuthBuffer.GetData(), plaintextAuthBuffer.GetBufferPosition(), aesCipher, &aesCipherSz, aesSessionKeySteam2, 16, aesIV, 16);

						char postCredentialsReplyData[512];
						BBWrite postCredentialsReplyBuffer(postCredentialsReplyData, 512);
						postCredentialsReplyBuffer.WriteNumber32BE(36);
						postCredentialsReplyBuffer.WriteBytes(aesIV, 16);
						postCredentialsReplyBuffer.WriteNumber16BE(12);
						postCredentialsReplyBuffer.WriteNumber16BE(16);
						postCredentialsReplyBuffer.WriteBytes(aesCipher, 16);

						if (skTCP.Send(postCredentialsReplyBuffer.GetData(), postCredentialsReplyBuffer.GetBufferPosition()))
						{
							printf("Sent post-credential reply\n");

							skTCP.SetExpectedPacket(k_ETCPPktTypeVerifyAuth);
						}
					}
					else if (expectedPacket == k_ETCPPktTypeVerifyAuth)
					{
						printf("Auth reply received\n");

						unsigned char cStatus = chDataBuffer.ReadByte();
						printf("Server replied with: %02X\n", cStatus);

						unsigned long long ulLoginTime = chDataBuffer.ReadNumber64LE();

						if (ulLoginTime > 0)
						{
							time_t tTime = (time_t)((ulLoginTime-UNIX_EPOCH)/1000000);
							printf("Login timestamp: %s\n", ctime(&tTime));
						}

						unsigned long long ulUnknown1 = chDataBuffer.ReadNumber64LE();

						switch (cStatus)
						{
							case 0:
							{
								printf("Success! Reading TGT packet..\n\n");

								unsigned long ulRemainingPacket = chDataBuffer.ReadNumber32BE();
								chDataBuffer.ReadNumber16BE(); //Unknown (00 02)
								
								unsigned char aesIV[16];
								chDataBuffer.ReadBytes((char *)aesIV, 16);

								unsigned short unPlainText = chDataBuffer.ReadNumber16BE();
								unsigned short unCipherText = chDataBuffer.ReadNumber16BE();
								unsigned char uchPlainText[512];
								unsigned char uchCipherText[512];
								
								printf("Cipher size: %d\n", unCipherText);
								printf("Plaintext size: %d\n", unPlainText);

								chDataBuffer.ReadBytes((char *)uchCipherText, unCipherText);

								AESDecrypt(uchCipherText, unCipherText, uchPlainText,  NULL, aesSessionKeySteam2, 16, aesIV, 16);

								printf("\nPlaintext hex: ");
								for (int i=0;i<unPlainText;i++)
									printf("%02X ", uchPlainText[i]);
								printf("\n\n");

								BBRead plainBuffer1((char *)uchPlainText, unPlainText);

								unsigned char aesBlobKey[16];

								plainBuffer1.ReadBytes((char *)aesBlobKey, 16);
								plainBuffer1.ReadNumber16LE(); //Unknown (00 00)

								TSteamGlobalUserID localUserID;
								localUserID.m_SteamLocalUserID.As64bits = plainBuffer1.ReadNumber64LE();

								clientSteamID.SetFromSteam2(&localUserID, k_EUniversePublic);
								
								printf("Steam ID: %s\n", clientSteamID.Render());

								plainBuffer1.ReadNumber16LE(); //Unknown (45 1C)
								plainBuffer1.ReadNumber32LE(); //Unknown (8C F7 9A 96)
								plainBuffer1.ReadNumber16LE(); //Unknown (45 1C)
								plainBuffer1.ReadNumber32LE(); //Unknown (99 52 96 69)

								unsigned long long ulTicketCreationTime = plainBuffer1.ReadNumber64LE();
								unsigned long long ulTicketExpirationTime = plainBuffer1.ReadNumber64LE();

								time_t tTicketCreationTime = (time_t)((ulTicketCreationTime-UNIX_EPOCH)/1000000);
								printf("Ticket creation time: %s", ctime(&tTicketCreationTime));

								time_t tTicketExpirationTime = (time_t)((ulTicketExpirationTime-UNIX_EPOCH)/1000000);
								printf("Ticket expiration time: %s\n", ctime(&tTicketExpirationTime));

								tgtServerReadable.m_unTicketSize = chDataBuffer.ReadNumber16BE();
								chDataBuffer.ReadBytes((char *)tgtServerReadable.m_rgchTicket, tgtServerReadable.m_unTicketSize);

								//HDR
								unsigned int unSizeOfWrapperBlob = chDataBuffer.ReadNumber32BE();
								chDataBuffer.ReadByte(); //01
								unsigned char ubPreprocessedFormatCode = chDataBuffer.ReadByte();
								unsigned int unSizeOfWrapperBlob2 = chDataBuffer.ReadNumber32LE();
								chDataBuffer.ReadNumber32LE(); //00 00 00 00
								//DATA
								unsigned int unSizeOfPlaintext = chDataBuffer.ReadNumber32LE();

								unsigned char aesBlobIV[16];
								chDataBuffer.ReadBytes((char *)aesBlobIV, 16);

								unsigned char aesBlobCipher[2048];
								unsigned int aesBlobCipherSz = chDataBuffer.GetRemainingBytes() - 40;

								printf("Cipher size: %d\n", aesBlobCipherSz);
								printf("Plaintext size: %d\n", unSizeOfPlaintext);

								chDataBuffer.ReadBytes((char *)aesBlobCipher, aesBlobCipherSz);

								unsigned char aesBlobPlaintext[2048];

								AESDecrypt(aesBlobCipher, aesBlobCipherSz, aesBlobPlaintext, NULL, aesBlobKey, 16, aesBlobIV, 16);

								printf("\nPlaintext hex: ");
								for (unsigned int i=0;i<unSizeOfPlaintext;i++)
									printf("%02X ", aesBlobPlaintext[i]);
								printf("\n\n");

								printf("Plaintext string: ");
								
								for (unsigned int i=0;i<unSizeOfPlaintext;i++)
								{
									char c = aesBlobPlaintext[i];

									if ((c >= 32) && (c <= 126))
										printf("%c", c);
									else
										printf(".");
								}

								printf("\n\n");

								printf("Remaining bytes: %d\n", chDataBuffer.GetRemainingBytes());

								bTrySteam3 = true;

								break;
							}
							case 1:
							{
								printf("Invalid account name specified\n\n");

								break;
							}
							case 2:
							{
								printf("Failed!\n\n");

								break;
							}
							case 3:
							{
								printf("Unknown, case 3\n\n");

								break;
							}
							case 4:
							{
								printf("The account specified is disabled\n\n");

								break;
							}
						}

						break;
					}
				}
				else
				{
					printf("recv error: %d\n", WSAGetLastError());

					return netManager.ShutdownWithMessage("TCP failed");
				}
			}
		}
		else
		{
			printf("send error: %d\n", WSAGetLastError());

			return netManager.ShutdownWithMessage("TCP failed");
		}
	}
	else
	{
		return netManager.ShutdownWithMessage("Fatal error, failed to bind TCP socket");
	}

	if (!bTrySteam3)
		return netManager.ShutdownWithMessage("Fatal Steam2 error");

	SteamSockUDP skUDP(saLocal);

	if (skUDP.IsBound())
	{
		printf("UDP socket initialized (%s)\n", saLocal.ToString());
		printf("Requesting challenge from %s\n", saTargetUDP.ToString());
		
		if (skUDP.SendTo(k_EUDPPktTypeChallengeReq, 0, NULL, 0, saTargetUDP))
		{
			while (true)
			{
				SteamAddr saSource;
				UDPPktHdr_t hdr;
				char chData[2048];
				unsigned int cchData = 0;

				if (skUDP.RecvFrom(&hdr, chData, 2048, &cchData, &saSource))
				{
					if (saSource.Matches(saTargetUDP))
					{
						if (hdr.m_nMagic == k_nMagic)
						{
							if (hdr.m_EUDPPktType == k_EUDPPktTypeChallenge)
							{
								printf("Challenge received\n");

								UDPPktTypeChallenge_t *pPktChallenge = (UDPPktTypeChallenge_t *)chData;

								UDPPktTypeConnect_t pktConnect;
								pktConnect.m_nObfuscatedChallenge = pPktChallenge->m_nChallenge ^ k_nChallengeMask;

								if (skUDP.SendTo(k_EUDPPktTypeConnect, 0, (const char *)&pktConnect, sizeof(UDPPktTypeConnect_t), saTargetUDP))
								{
									printf("Connecting to server...\n");
								}
								else
								{
									printf("Failed to connect to server\n");
								
									break;
								}
							}
							else if (hdr.m_EUDPPktType == k_EUDPPktTypeAccept)
							{
								printf("Accepted by server\n");
							}
							else if (hdr.m_EUDPPktType == k_EUDPPktTypeDisconnect)
							{
								printf("Dropped by server\n");

								skUDP.SendTo(k_EUDPPktTypeDisconnect, 4, NULL, 0, saTargetUDP);

								break;
							}
							else if (hdr.m_EUDPPktType == k_EUDPPktTypeData)
							{
								printf("Data received\n");

								if (bChannelEncryption)
								{
									unsigned char ubDecryptedData[2056];
									unsigned int cubDecryptedDataSz = 0;

									try
									{
										if (!SymmetricDecrypt((const unsigned char *)chData, cchData, ubDecryptedData, &cubDecryptedDataSz, aesSessionKeySteam3, 32))
										{
											printf("AES decryption failed\n");
										
											break;
										}
									}
									catch (...)
									{
										printf("AES decryption exception\n");

										break;
									}

									MsgHdr_t *pMsgHdr = (MsgHdr_t *)ubDecryptedData;

									printf("Encrypted EMsg: %d\n", pMsgHdr->m_EMsg);

									if (pMsgHdr->m_EMsg == k_EMsgMulti)
									{
										MsgMulti_t *pMultiMsg = (MsgMulti_t *)(ubDecryptedData + sizeof(MsgHdr_t));

										if (pMultiMsg->m_cubUnzipped == 0)
										{
											ExtendedClientMsgHdr_t *pMsgExtendedHdr = (ExtendedClientMsgHdr_t *)(ubDecryptedData + sizeof(MsgHdr_t) + sizeof(MsgMulti_t) + 4);

											printf("MsgMulti EMsg: %d\n", pMsgExtendedHdr->m_EMsg);

											if (pMsgExtendedHdr->m_EMsg == k_EMsgClientServersAvailable)
											{
												printf("Received k_MsgClientServersAvailable\n");
											}
											else if (pMsgExtendedHdr->m_EMsg == k_EMsgClientLogOnResponse)
											{
												MsgClientLogOnResponse_t *pMsg = (MsgClientLogOnResponse_t *)(ubDecryptedData + sizeof(MsgHdr_t) + sizeof(MsgMulti_t) + 4 + sizeof(ExtendedClientMsgHdr_t));

												printf("Received logon response, EResult=%d\n", pMsg->m_EResult);
											}
										}
										else
										{
											printf("Compressed packet received (size=%d)\n", pMultiMsg->m_cubUnzipped);
										}
									}
								}
								else if (!bChannelEncryption)
								{
									MsgHdr_t *pMsgHdr = (MsgHdr_t *)chData;

									printf("EMsg: %d\n", pMsgHdr->m_EMsg);

									if (pMsgHdr->m_EMsg == k_EMsgChannelEncryptRequest)
									{
										printf("Encryption request!\n");

										MsgChannelEncryptRequest_t *pEncryptRequest = (MsgChannelEncryptRequest_t *)(chData + sizeof(MsgHdr_t));

										if ((pEncryptRequest->m_unProtocolVer == 1) && (pEncryptRequest->m_EUniverse == 1))
										{
#pragma pack(push, 1)
											struct : public MsgHdr_t, MsgChannelEncryptResponse_t
											{
												char m_pubEncryptedKey[128];
													
												uint32 m_unCRC;
												uint32 m_unMystery;
											} encryptResponseFull;
#pragma pack(pop)
											//MsgHdr_t
											encryptResponseFull.m_EMsg = k_EMsgChannelEncryptResponse;
											encryptResponseFull.m_JobIDTarget = -1;
											encryptResponseFull.m_JobIDSource = -1;
											//MsgChannelEncryptResponse_t
											encryptResponseFull.m_unProtocolVer = 1;
											encryptResponseFull.m_cubEncryptedKey = 128;

											GenerateRandomBlock(aesSessionKeySteam3, 32);

											char rsaEncryptedSessionKey[128];
											unsigned int rsaEncryptedSessionKeySz = 128;

											if (RSAEncrypt(aesSessionKeySteam3, 32, (unsigned char *)rsaEncryptedSessionKey, &rsaEncryptedSessionKeySz, k_rgchPublicKey_Public, 160))
											{
												memcpy(encryptResponseFull.m_pubEncryptedKey, rsaEncryptedSessionKey, 128);

												encryptResponseFull.m_unCRC = CRC32_ProcessSingleBuffer(rsaEncryptedSessionKey, 128);
												encryptResponseFull.m_unMystery = 0;

												if (skUDP.SendTo(k_EUDPPktTypeData, 4, (const char *)&encryptResponseFull, sizeof(encryptResponseFull), saTargetUDP))
												{
													printf("Sent encrypted session key to server\n");
												}
												else
												{
													printf("Failed to send encrypted session key to server\n");

													break;
												}
											}
											else
											{
												printf("RSA encryption failed\n");

												break;
											}
										}
									}
									else if (pMsgHdr->m_EMsg == k_EMsgChannelEncryptResult)
									{
										MsgChannelEncryptResult_t *pEncryptResult = (MsgChannelEncryptResult_t *)(chData + sizeof(MsgHdr_t));

										if (pEncryptResult->m_EResult == k_EResultOK)
										{
#pragma pack(push, 1)
											struct : public ExtendedClientMsgHdr_t, MsgClientRegisterAuthTicketWithCM_t
											{
											} registerAuthTicket;
#pragma pack(pop)
											//ExtendedClientMsgHdr_t
											registerAuthTicket.m_EMsg = k_EMsgClientRegisterAuthTicketWithCM;
											registerAuthTicket.m_nCubHdr = 0x24;
											registerAuthTicket.m_nHdrVersion = 0x02;
											registerAuthTicket.m_JobIDTarget = -1;
											registerAuthTicket.m_JobIDSource = -1;
											registerAuthTicket.m_nHdrCanary = 0xEF;
											registerAuthTicket.m_ulSteamID = clientSteamID.ConvertToUint64();
											registerAuthTicket.m_nSessionID = 0;

											registerAuthTicket.m_unProtocolVer = 0x0001001B;
											registerAuthTicket.m_unTicketLengthWithSignature = hardcodedAuthTicketSz;
											
											unsigned char ubAuthData[2048];

											BBWrite registerAuthTicketBuffer((char *)ubAuthData, 2048);
											registerAuthTicketBuffer.WriteBytes((char *)&registerAuthTicket, sizeof(registerAuthTicket));
											registerAuthTicketBuffer.WriteBytes((char *)hardcodedAuthTicket, hardcodedAuthTicketSz);

											printf("RegisterAuthTicket hex: ");
											for (unsigned int i=0;i<registerAuthTicketBuffer.GetBufferPosition();i++)
												printf("%02X ", ubAuthData[i]);
											printf("\n\n");

											unsigned char ubEncryptedAuthData[2048];
											unsigned int cubEncryptedAuthData = 0;

											if (SymmetricEncrypt(ubAuthData, registerAuthTicketBuffer.GetBufferPosition(), ubEncryptedAuthData, &cubEncryptedAuthData, aesSessionKeySteam3, 32))
											{
												printf("RegisterAuthTicket cipher size: %d\n", cubEncryptedAuthData);
												printf("RegisterAuthTicket cipher: ");
												for (unsigned int i=0;i<cubEncryptedAuthData;i++)
													printf("%02X ", ubEncryptedAuthData[i]);
												printf("\n\n");

												if (!skUDP.SendTo(k_EUDPPktTypeData, 4, (const char *)&ubEncryptedAuthData, cubEncryptedAuthData, saTargetUDP))
												{
													printf("Failed to send encrypted EMsg to server\n");
													
													break;
												}
											}
											else
											{
												printf("AES encryption failed!\n");

												break;
											}

#pragma pack(push, 1)
											struct : public ExtendedClientMsgHdr_t, MsgClientLogOnWithCredentials_t
											{
											} msgLogOnWithCredentials;
#pragma pack(pop)
											msgLogOnWithCredentials.m_EMsg = k_EMsgClientLogOnWithCredentials;
											msgLogOnWithCredentials.m_nCubHdr = 0x24;
											msgLogOnWithCredentials.m_nHdrVersion = 0x02;
											msgLogOnWithCredentials.m_JobIDTarget = -1;
											msgLogOnWithCredentials.m_JobIDSource = -1;
											msgLogOnWithCredentials.m_nHdrCanary = 0xEF;
											msgLogOnWithCredentials.m_ulSteamID = clientSteamID.ConvertToUint64();
											msgLogOnWithCredentials.m_nSessionID = 0;

											msgLogOnWithCredentials.m_unProtocolVer = 0x0001001B;
											msgLogOnWithCredentials.m_unIPPrivateObfuscated = 0x7A05F104;
											msgLogOnWithCredentials.m_unIPPublic = 0;
											msgLogOnWithCredentials.m_ulClientSuppliedSteamId = 0;
											msgLogOnWithCredentials.m_unTicketLength = tgtServerReadable.m_unTicketSize + 4;
											
											memset(msgLogOnWithCredentials.m_rgchAccountName, 0, 64);
											memcpy(msgLogOnWithCredentials.m_rgchAccountName, chClientUsername, strlen(chClientUsername));

											memset(msgLogOnWithCredentials.m_rgchPassword, 0, 20);
											memcpy(msgLogOnWithCredentials.m_rgchPassword, chClientPassword, strlen(chClientPassword));

											msgLogOnWithCredentials.m_qosLevel = 0;

											unsigned char ubPlaintextData[2048];

											BBWrite ubPlaintextBuffer((char *)ubPlaintextData, 2048);
											ubPlaintextBuffer.WriteBytes((char *)&msgLogOnWithCredentials, sizeof(msgLogOnWithCredentials));
											ubPlaintextBuffer.WriteNumber32BE(nClientInternalIP);
											ubPlaintextBuffer.WriteBytes(tgtServerReadable.m_rgchTicket, tgtServerReadable.m_unTicketSize);
											ubPlaintextBuffer.WriteString("christopher.thorne@live.co.uk");
											ubPlaintextBuffer.WriteString("english");
											ubPlaintextBuffer.WriteNumber32LE(0x4A85D6BE);
											ubPlaintextBuffer.WriteNumber32LE(1);
											ubPlaintextBuffer.WriteByte(0);
											ubPlaintextBuffer.WriteString("MessageObject");
											ubPlaintextBuffer.WriteBytes(hardcodedUnknownMessageObject, 163); //TODO

											printf("LogOnWithCredentials hex: ");
											for (unsigned int i=0;i<ubPlaintextBuffer.GetBufferPosition();i++)
												printf("%02X ", ((unsigned char *)ubPlaintextBuffer.GetData())[i]);
											printf("\n\n");

											unsigned char ubEncryptedData[2048];
											unsigned int cubEncryptedData = 0;

											if (SymmetricEncrypt((const unsigned char *)&msgLogOnWithCredentials, ubPlaintextBuffer.GetBufferPosition(), ubEncryptedData, &cubEncryptedData, aesSessionKeySteam3, 32))
											{
												printf("LogOnWithCredentials cipher size: %d\n", cubEncryptedData);
												printf("LogOnWithCredentials cipher: ");
												for (unsigned int i=0;i<cubEncryptedData;i++)
													printf("%02X ", ubEncryptedData[i]);
												printf("\n\n");

												if (skUDP.SendTo(k_EUDPPktTypeData, 4, (const char *)&ubEncryptedData, cubEncryptedData, saTargetUDP))
												{
													printf("Encryption handshake completed!\n");

													bChannelEncryption = true;
												}
												else
												{
													printf("Failed to send encrypted EMsg to server\n");
												}
											}
											else
											{
												printf("AES encryption failed!\n");

												break;
											}
										}
									}
								}
							}
							else if (hdr.m_EUDPPktType == k_EUDPPktTypeDatagram)
							{
								printf("Datagram received\n", hdr.m_EUDPPktType);
							}
						}
						else
						{
							printf("Invalid magic value received\n");
						}
					}
					else
					{
						printf("Packet from unknown sender (%s), ignoring\n", saSource.ToString());
					}
				}
				else
				{
					printf("recvfrom error: %d\n", WSAGetLastError());
				}
			}
		}
		else
		{
			printf("sendto: %d\n", WSAGetLastError());
		}
	}
	else
	{
		printf("bind error: %d\n", WSAGetLastError());
	}

	return netManager.ShutdownWithMessage("Press any key to exit...");
}