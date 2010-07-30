#define DEFAULT_PORT 50000

#include "SteamCrypto.h"
#include "SteamSock.h"
#include "EMessages.h"

#include "tier1/checksum_crc.h"

#include <stdio.h>
#include <conio.h>

bool bChannelEncryption = false;

SteamNetManager netManager;

unsigned char k_rgchPublicKey_Public[160] = {
    0x30, 0x81, 0x9D, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00, 0x03, 0x81, 0x8B, 0x00, 0x30, 0x81, 0x87, 0x02, 0x81, 0x81, 0x00, 0xDF, 0xEC, 0x1A,
    0xD6, 0x2C, 0x10, 0x66, 0x2C, 0x17, 0x35, 0x3A, 0x14, 0xB0, 0x7C, 0x59, 0x11, 0x7F, 0x9D, 0xD3, 0xD8, 0x2B, 0x7A, 0xE3, 0xE0, 0x15, 0xCD, 0x19, 0x1E, 0x46, 0xE8, 0x7B, 0x87, 0x74, 0xA2, 0x18,
    0x46, 0x31, 0xA9, 0x03, 0x14, 0x79, 0x82, 0x8E, 0xE9, 0x45, 0xA2, 0x49, 0x12, 0xA9, 0x23, 0x68, 0x73, 0x89, 0xCF, 0x69, 0xA1, 0xB1, 0x61, 0x46, 0xBD, 0xC1, 0xBE, 0xBF, 0xD6, 0x01, 0x1B, 0xD8,
    0x81, 0xD4, 0xDC, 0x90, 0xFB, 0xFE, 0x4F, 0x52, 0x73, 0x66, 0xCB, 0x95, 0x70, 0xD7, 0xC5, 0x8E, 0xBA, 0x1C, 0x7A, 0x33, 0x75, 0xA1, 0x62, 0x34, 0x46, 0xBB, 0x60, 0xB7, 0x80, 0x68, 0xFA, 0x13,
    0xA7, 0x7A, 0x8A, 0x37, 0x4B, 0x9E, 0xC6, 0xF4, 0x5D, 0x5F, 0x3A, 0x99, 0xF9, 0x9E, 0xC4, 0x3A, 0xE9, 0x63, 0xA2, 0xBB, 0x88, 0x19, 0x28, 0xE0, 0xE7, 0x14, 0xC0, 0x42, 0x89, 0x02, 0x01, 0x11,
};

unsigned char aesSessionKey[32];

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
    netManager.Initialize();

    char cmdIP[16];
    unsigned long sckIP = 0;

    if (CMD_GetKeyValue(argc, argv, "-ip", cmdIP))
    {
        sckIP = inet_addr(cmdIP);
    }
    else if (!UTIL_GetLocalAddress(&sckIP))
    {
        return netManager.ShutdownWithMessage("Fatal error, failed to get local address");
    }

    char cmdPort[8];
    unsigned short sckPort = 0;

    if (CMD_GetKeyValue(argc, argv, "-port", cmdPort))
    {
        sckPort = htons(atoi(cmdPort));
    }
    else
    {
        sckPort = htons(DEFAULT_PORT);
    }

    sockaddr_in local;
    local.sin_addr.s_addr = sckIP;
    local.sin_port = sckPort;

    SteamAddr saTargetTCP("72.165.61.138", 27039);
    SteamAddr saTargetUDP("68.142.64.165", 27017);
    SteamAddr saLocal(local);

    SteamSockTCP skTCP(saLocal, saTargetTCP);

    if (skTCP.IsBound())
    {
        printf("TCP socket initialized (%s)\n", saLocal.ToString());
        printf("Initiating connection with %s\n", saTargetTCP.ToString());

        TCPPktTypeRequestExternalIP_t requestExternalIP;
        requestExternalIP.m_nUnknown1 = 0;
        requestExternalIP.m_cUnknown2 = 4;
        requestExternalIP.m_nInternalIP = sckIP;
        requestExternalIP.m_nUnknown3 = ntohl(1);

        if (skTCP.Send((const char *)&requestExternalIP, sizeof(TCPPktTypeRequestExternalIP_t)))
        {
            skTCP.SetExpectedPacket(k_ETCPPktTypeExternalIP);

            while (true)
            {
                char chData[2048];
                unsigned int cchData = 0;

                if (skTCP.Recv(chData, 2048, &cchData))
                {
                    printf("Received TCP packet!\n");

                    if (skTCP.GetExpectedPacket() == k_ETCPPktTypeExternalIP)
                    {
                        printf("TCPPktTypeExternalIP\n");

                        TCPPktTypeExternalIP_t *pExternalIP = (TCPPktTypeExternalIP_t *)chData;

                        if (pExternalIP->m_nUnknown1 == 0)
                        {
                            IN_ADDR extTest;
                            extTest.s_addr = htonl(pExternalIP->m_nExternalIP);

                            printf("External IP: %s\n", inet_ntoa(extTest));

                            break;
                        }
                    }
                }
                else
                {
                    printf("recv error: %d\n", WSAGetLastError());
                }
            }
        }
        else
        {
            printf("send error: %d\n", WSAGetLastError());
        }
    }
    else
    {
        return netManager.ShutdownWithMessage("Fatal error, failed to bind TCP socket");
    }

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
                                printf("Challenge received!\n");

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

                                break;
                            }
                            else if (hdr.m_EUDPPktType == k_EUDPPktTypeData)
                            {
                                printf("Data received\n");

                                if (bChannelEncryption)
                                {
                                    unsigned char ubDecryptedData[2056];
                                    unsigned int cubDecryptedDataSz = 0;

                                    if (SymmetricDecrypt((const unsigned char *)chData, cchData, ubDecryptedData, &cubDecryptedDataSz, aesSessionKey, 32))
                                    {
                                        ExtendedClientMsgHdr_t *pMsgHdr = (ExtendedClientMsgHdr_t *)ubDecryptedData;

                                        printf("Encrypted EMsg: %d\n", pMsgHdr->m_EMsg);

                                        break;
                                    }
                                    else
                                    {
                                        printf("AES decryption failed\n");
                                        
                                        break;
                                    }
                                }
                                else if (!bChannelEncryption)
                                {
                                    MsgHdr_t *pMsgHdr = (MsgHdr_t *)chData;

                                    printf("EMsg: %d\n", pMsgHdr->m_EMsg);

                                    if (pMsgHdr->m_EMsg == k_EMsgChannelEncryptRequest)
                                    {
                                        printf("Encryption request!\n");

                                        MsgChannelEncryptRequest_t *pEncryptRequest = (MsgChannelEncryptRequest_t *)(chData+sizeof(MsgHdr_t));

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

                                            GenerateRandomBlock(aesSessionKey, 32);

                                            char rsaEncryptedSessionKey[128];
                                            unsigned int rsaEncryptedSessionKeySz = 128;

                                            if (RSAEncrypt(aesSessionKey, 32, (unsigned char *)rsaEncryptedSessionKey, &rsaEncryptedSessionKeySz, k_rgchPublicKey_Public, 160))
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
                                        MsgChannelEncryptResult_t *pEncryptResult = (MsgChannelEncryptResult_t *)(chData+sizeof(MsgHdr_t));

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
                                            registerAuthTicket.m_ulSteamID = -1; //TODO (Steam2?)
                                            registerAuthTicket.m_nSessionID = -1; //TODO (Steam2?)
                                            //MsgClientRegisterAuthTicketWithCM_t
                                            registerAuthTicket.m_nUnknown1 = 0x1B;
                                            registerAuthTicket.m_nUnknown2 = 0x01;
                                            registerAuthTicket.m_unTicketLengthWithSignature = 0xB0;
                                            registerAuthTicket.m_unTicketLengthWithoutSignature = 0x30;
                                            registerAuthTicket.m_unTicketVersion = 0x04;
                                            registerAuthTicket.m_ulTicketSteamID = -1;
                                            registerAuthTicket.m_unUnknown3 = 0x07;
                                            registerAuthTicket.m_unExternalIP = -1;
                                            registerAuthTicket.m_unInternalIP = -1;
                                            registerAuthTicket.m_unUnknown4 = 0;

                                            memset(registerAuthTicket.m_rgubUnknown5, 0, 128);

                                            unsigned char ubEncryptedData[2048];
                                            unsigned int cubEncryptedData = 0;

                                            if (SymmetricEncrypt((const unsigned char *)&registerAuthTicket, sizeof(registerAuthTicket), ubEncryptedData, &cubEncryptedData, aesSessionKey, 32))
                                            {
                                                printf("Encryption handshake completed, breaking!\n");

                                                break;

                                                /*if (skUDP.SendTo(k_EUDPPktTypeData, 4, (const char *)&ubEncryptedData, cubEncryptedData, saTarget))
                                                {
                                                    printf("Encryption handshake completed, sent MsgClientRegisterAuthTicketWithCM_t!\n");

                                                    bChannelEncryption = true;
                                                }
                                                else
                                                {
                                                    printf("Failed to send encrypted MsgClientRegisterAuthTicketWithCM_t to server\n");
                                                }*/
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