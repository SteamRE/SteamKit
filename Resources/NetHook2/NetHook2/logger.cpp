#include <cstdio>
#include <ctime>
#include <sstream>

#ifdef __linux__
    #include <cstdarg>
    #include <limits>
#elif _WIN32
    #include <Windows.h>
    #define PATH_MAX MAX_PATH
#endif

#include "logger.h"
#include "clientmodule.h"
#include "crypto.h"
#include "zip.h"
#include "binaryreader.h"
#include "utils.h"

#include "steammessages_base.pb.h"

namespace NetHook
{

CLogger::CLogger() noexcept:
    m_uiMsgNum(0)
{
}

CLogger::~CLogger()
{
}

void CLogger::InitSessionLogDir()
{
    m_RootDir = Utils::GetNetHookLogDirOverride();
    if(m_RootDir.empty())
    {
        m_RootDir = g_pClientModule->GetDirectory();
        if(m_RootDir.empty())
        {
            m_RootDir = Utils::GetCurrentWorkDir();
        }
    }

#ifdef __linux__
    m_RootDir += "//nethook//";
#elif _WIN32
    m_RootDir += "\\nethook\\";
#endif

    // create root nethook log directory if it doesn't exist
    Utils::MkDir( m_RootDir.c_str() );

    time_t currentTime;
    time( &currentTime );

    std::ostringstream ss;
    ss << m_RootDir << currentTime;
#ifdef _WIN32
    ss << '\\';
#endif
    m_LogDir = ss.str();

    // create the session log directory
    Utils::MkDir(m_LogDir.c_str());
#ifdef __linux__
    m_LogDir += '/';
#endif
}

void CLogger::LogConsole( const char *szFmt, ... )
{
#ifdef __linux__
    va_list args;
    va_start( args, szFmt );
    vprintf(szFmt, args);
    va_end(args);
#elif _WIN32
    va_list args;
    va_start( args, szFmt );

    int buffSize = _vscprintf( szFmt, args ) + 1;

    if ( buffSize <= 1 )
        return;

    char *szBuff = new char[ buffSize ];
    memset( szBuff, 0, buffSize );

    const int len = vsprintf_s( szBuff, buffSize, szFmt, args );

    szBuff[ buffSize - 1 ] = 0;

    HANDLE hOutput = GetStdHandle( STD_OUTPUT_HANDLE );

    DWORD numWritten = 0;
    WriteFile( hOutput, szBuff, len, &numWritten, nullptr );

    delete [] szBuff;
#endif
}

void CLogger::LogNetMessage( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
    EMsg eMsg = (EMsg)*(uint16*)pData;
    eMsg = (EMsg)((int)eMsg & (~0x80000000));

    if ( eMsg == EMsg::k_EMsgMulti )
    {
        this->MultiplexMulti( eDirection, pData, cubData );
        return;
    }

    this->LogSessionData( eDirection, pData, cubData );
}

void CLogger::LogSessionData( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
    std::string fullFile = m_LogDir;

    const char *outFile = GetFileNameBase( eDirection, (EMsg)*(uint16*)pData );
    fullFile += outFile;

    std::string fullFileTmp = fullFile + ".tmp";
    std::string fullFileFinal = fullFile + ".bin";

#ifdef __linux__
    FILE* hFile = fopen( fullFileTmp.c_str(), "wb+");
    fwrite(pData, sizeof(char), cubData, hFile);
    fclose( hFile );
#elif _WIN32
    HANDLE hFile = CreateFile( fullFileTmp.c_str(), GENERIC_WRITE, FILE_SHARE_READ, nullptr, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr );
    DWORD numBytes = 0;
    WriteFile( hFile, pData, cubData, &numBytes, nullptr );
    CloseHandle( hFile );
#endif

    Utils::RenameFile(fullFileTmp.c_str(), fullFileFinal.c_str());

    this->LogConsole( "Wrote %d bytes to %s\n", cubData, outFile );
}

const char *CLogger::GetFileNameBase( ENetDirection eDirection, EMsg eMsg, uint8 serverType )
{
    static char szFileName[PATH_MAX];
#ifdef __linux__
    sprintf(
        szFileName,
        "%03u_%s_%d_%s",
        ++m_uiMsgNum,
        ( eDirection == ENetDirection::k_eNetIncoming ? "in" : "out" ),
        static_cast<int>( eMsg ),
        g_pCrypto->GetPchMessage( eMsg, serverType )
    );
#elif _WIN32
    sprintf_s(
        szFileName, sizeof( szFileName ),
        "%03u_%s_%d_%s",
        ++m_uiMsgNum,
        ( eDirection == ENetDirection::k_eNetIncoming ? "in" : "out" ),
        static_cast<int>( eMsg ),
        g_pCrypto->GetPchMessage( eMsg, serverType )
    );
#endif

    return szFileName;
}

void CLogger::MultiplexMulti( ENetDirection eDirection, const uint8 *pData, uint32 cubData )
{
    struct ProtoHdr
    {
        EMsg msg;
        int headerLength;
    };


    const ProtoHdr *pProtoHdr = (ProtoHdr*) pData;

    this->LogConsole("Multi: msg %d length %d\n", (static_cast<int>(pProtoHdr->msg) & (~0x80000000)), pProtoHdr->headerLength );

    CMsgProtoBufHeader protoheader;
    protoheader.ParseFromArray( pData + 8, pProtoHdr->headerLength );

    this->LogConsole("MultiProto\n");

    CMsgMulti multi;
    multi.ParseFromArray( pData + 8 + pProtoHdr->headerLength, cubData - 8 - pProtoHdr->headerLength );

    this->LogConsole("MultiMsg: %d %d\n", multi.size_unzipped(), multi.message_body().length() );

    uint8 *pMsgData = nullptr;
    uint32 cubMsgData = 0;
    bool bDecomp = false;

    if ( multi.has_size_unzipped() && multi.size_unzipped() != 0 )
    {
        // decompress our data

        uint8 *pDecompressed = new uint8[ multi.size_unzipped() ];
        uint8 *pCompressed = (uint8 *)( multi.message_body().c_str() );
        const uint32 cubCompressed = multi.message_body().length();

        this->LogConsole("decomp: %x comp: %x cubcomp: %d unzipped: %d\n", pDecompressed, pCompressed, cubCompressed, multi.size_unzipped());

        const bool bZip = CZip::Inflate( pCompressed, cubCompressed, pDecompressed, multi.size_unzipped() );

        if ( !bZip )
        {
            delete [] pDecompressed;

            this->LogConsole("Unable to decompress buffer\n");
            return;
        }

        pMsgData = pDecompressed;
        cubMsgData = multi.size_unzipped();
        bDecomp = bZip;
    }
    else
    {
        pMsgData = (uint8 *)( multi.message_body().c_str() );
        cubMsgData = multi.message_body().length();
    }

    CBinaryReader reader( pMsgData, cubMsgData );

    while ( reader.GetSizeLeft() > 0 )
    {
        const uint32 cubPayload = reader.Read<uint32>();
        const uint8 *pPayload = reader.ReadBytes( cubPayload );

        this->LogNetMessage( eDirection, pPayload, cubPayload );
    }

    if ( bDecomp )
        delete [] pMsgData;
}

}
