#include "ByteBuffer.h"
#include "NumberUtil.h"
#include "StringUtil.h"

bool BBShared::SetData(char *pchData, unsigned int cchData)
{
	m_nBufferPosition = 0;

	if (pchData)
	{
		m_pchData = pchData;
		m_cchData = cchData;

		return (m_bDataSet = true);
	}
	else
	{
		return (m_bDataSet = false);
	}
}

char *BBShared::GetData()
{
	return m_pchData;
}

unsigned int BBShared::GetRemainingBytes()
{
	return GetBufferSize()-GetBufferPosition();
}

unsigned int BBShared::GetBufferSize()
{
	return m_cchData;
}

unsigned int BBShared::GetBufferPosition()
{
	return m_nBufferPosition;
}

void BBShared::SetBufferPosition(unsigned int nPosition)
{
	m_nBufferPosition = nPosition;
}

BBRead::BBRead(char *pchData, unsigned int cchData)
{
	SetData(pchData, cchData);
}

char BBRead::ReadByte(bool bPeeking)
{
	if (!m_pchData)
		return 0;

	if (m_nBufferPosition >= m_cchData)
		return 0;

	char cReturn = m_pchData[m_nBufferPosition];

	if (!bPeeking)
		m_nBufferPosition++;

	return cReturn;
}

bool BBRead::ReadBytes(char *pchData, unsigned int cchData)
{
	if (!pchData)
		return false;

	if (cchData == 0)
		return false;

	for (unsigned int i=0;i<cchData;i++)
		pchData[i] = ReadByte();

	return true;
}

bool BBRead::ReadBool()
{
	return (bool)ReadByte();
}

char BBRead::ReadChar()
{
	return ReadByte();
}

unsigned short BBRead::ReadNumber16LE()
{
	char chData[2];
	ReadBytes(chData, 2);

	return *((unsigned short *)&chData);
}

unsigned int BBRead::ReadNumber32LE()
{
	char chData[4];
	ReadBytes(chData, 4);

	return *((unsigned int *)&chData);
}

unsigned long long BBRead::ReadNumber64LE()
{
	char chData[8];
	ReadBytes(chData, 8);

	return *((unsigned long long *)&chData);
}

unsigned short BBRead::ReadNumber16BE()
{
	return EndianSwap16(ReadNumber16LE());
}

unsigned int BBRead::ReadNumber32BE()
{
	return EndianSwap32(ReadNumber32LE());
}

unsigned long long BBRead::ReadNumber64BE()
{
	return EndianSwap64(ReadNumber64LE());
}

unsigned int BBRead::ReadString(char *pchString, unsigned int cchString)
{
	unsigned int nWritten = 0;

	while (ReadByte(true) != '\0')
	{
		pchString[nWritten++] = ReadByte();

		if (nWritten >= cchString)
			break;
	}

	return nWritten;
}

BBWrite::BBWrite(char *pchData, unsigned int cchData)
{
	SetData(pchData, cchData);
}

bool BBWrite::WriteByte(char cValue)
{
	if (!m_pchData)
		return false;
	
	if (m_nBufferPosition >= m_cchData)
		return false;

	m_pchData[m_nBufferPosition++] = cValue;

	return true;
}

bool BBWrite::WriteBytes(char *pchData, unsigned int cchData)
{
	if (!pchData)
		return false;

	if (cchData == 0)
		return false;

	for (unsigned int i=0;i<cchData;i++)
	{
		if (!WriteByte(pchData[i]))
			return false;
	}

	return true;
}

bool BBWrite::WriteBytes(unsigned char *pubData, unsigned int cubData)
{
	return WriteBytes((char *)pubData, cubData);
}

bool BBWrite::WriteBool(bool bValue)
{
	return WriteByte((char)bValue);
}

bool BBWrite::WriteChar(char cValue)
{
	return WriteByte(cValue);
}

bool BBWrite::WriteNumber16LE(unsigned short nValue)
{
	return WriteBytes((char *)&nValue, 2);
}

bool BBWrite::WriteNumber32LE(unsigned int nValue)
{
	return WriteBytes((char *)&nValue, 4);
}

bool BBWrite::WriteNumber64LE(unsigned long long nValue)
{
	return WriteBytes((char *)&nValue, 8);
}

bool BBWrite::WriteNumber16BE(unsigned short nValue)
{
	return WriteNumber16LE(EndianSwap16(nValue));
}

bool BBWrite::WriteNumber32BE(unsigned int nValue)
{
	return WriteNumber32LE(EndianSwap32(nValue));
}

bool BBWrite::WriteNumber64BE(unsigned long long nValue)
{
	return WriteNumber64LE(EndianSwap64(nValue));
}

bool BBWrite::WriteString(const char *pchString, unsigned int cchString)
{
	if (cchString == 0)
		cchString = StringLength(pchString);

	if (!WriteBytes((char *)pchString, cchString))
		return false;

	if (!WriteChar('\0'))
		return false;

	return true;
}