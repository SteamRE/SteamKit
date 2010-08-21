#ifndef BYTEBUFFER_H
#define BYTEBUFFER_H

class BBShared
{
public:
	bool SetData(char *pchData, unsigned int cchData);
	char *GetData();

	unsigned int GetRemainingBytes();
	unsigned int GetBufferSize();

	unsigned int GetBufferPosition();
	void SetBufferPosition(unsigned int nPosition);
protected:
	char *m_pchData;
	unsigned int m_cchData;
	bool m_bDataSet;

	unsigned int m_nBufferPosition;
};

class BBRead : public BBShared
{
public:
	BBRead(char *pchData, unsigned int cchData);

	char ReadByte(bool bPeeking = false);
	bool ReadBytes(char *pchData, unsigned int cchData);

	bool ReadBool();
	char ReadChar();

	unsigned short ReadNumber16LE();
	unsigned int ReadNumber32LE();
	unsigned long long ReadNumber64LE();

	unsigned short ReadNumber16BE();
	unsigned int ReadNumber32BE();
	unsigned long long ReadNumber64BE();

	unsigned int ReadString(char *pchString, unsigned int cchString);
};

class BBWrite : public BBShared
{
public:
	BBWrite(char *pchData, unsigned int cchData);

	bool WriteByte(char cValue);
	bool WriteBytes(char *pchData, unsigned int cchData);
	bool WriteBytes(unsigned char *pubData, unsigned int cubData);

	bool WriteBool(bool bValue);
	bool WriteChar(char cValue);

	bool WriteNumber16LE(unsigned short nValue);
	bool WriteNumber32LE(unsigned int nValue);
	bool WriteNumber64LE(unsigned long long nValue);

	bool WriteNumber16BE(unsigned short nValue);
	bool WriteNumber32BE(unsigned int nValue);
	bool WriteNumber64BE(unsigned long long nValue);

	bool WriteString(const char *pchString, unsigned int cchString = 0);
};

#endif //BYTEBUFFER_H