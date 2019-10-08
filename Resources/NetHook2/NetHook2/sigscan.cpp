
#include "sigscan.h"
 
/* There is no ANSI ustrncpy */
unsigned char* ustrncpy(unsigned char *dest, const unsigned char *src, int len) noexcept {
    while(len--)
        dest[len] = src[len];
 
    return dest;
}
 
/* //////////////////////////////////////
    CSigScan Class
    ////////////////////////////////////// */
unsigned char* CSigScan::base_addr;
size_t CSigScan::base_len;
void *(*CSigScan::sigscan_dllfunc)(const char *pName, int *pReturnCode);
 
/* Initialize the Signature Object */
int CSigScan::Init(const unsigned char *sig, const char *mask, size_t len) {
    is_set = 0;
 
    sig_len = len;

	if ( sig_str )
		delete[] sig_str;

    sig_str = new unsigned char[sig_len];
    ustrncpy(sig_str, sig, sig_len);
 
	if ( sig_mask )
		delete[] sig_mask;

    sig_mask = new char[sig_len + 1];
    strncpy_s(sig_mask, sig_len + 1, mask, sig_len);
 
    if(!base_addr)
        return 2; // GetDllMemInfo() Failed
 
    if((sig_addr = FindSignature()) == nullptr)
        return 1; // FindSignature() Failed
 
    is_set = 1;
    // SigScan Successful!

	return 0;
}
 
/* Destructor frees sig-string allocated memory */
CSigScan::~CSigScan(void) {
    delete[] sig_str;
    delete[] sig_mask;
}
 
/* Get base address of the server module (base_addr) and get its ending offset (base_len) */
bool CSigScan::GetDllMemInfo(void) noexcept {
    void *pAddr = (void*)sigscan_dllfunc;
    base_addr = nullptr;
    base_len = 0;
 
    #ifdef WIN32
    MEMORY_BASIC_INFORMATION mem = { };
 
    if(!pAddr)
        return false; // GetDllMemInfo failed!pAddr
 
    if(!VirtualQuery(pAddr, &mem, sizeof(mem)))
        return false;
 
    base_addr = (unsigned char*)mem.AllocationBase;
 
    const IMAGE_DOS_HEADER * dos = (IMAGE_DOS_HEADER*)mem.AllocationBase;
    const IMAGE_NT_HEADERS * pe = (IMAGE_NT_HEADERS*)((unsigned long)dos+(unsigned long)dos->e_lfanew);
 
    if(pe->Signature != IMAGE_NT_SIGNATURE) {
        base_addr = nullptr;
        return false; // GetDllMemInfo failedpe points to a bad location
    }
 
    base_len = (size_t)pe->OptionalHeader.SizeOfImage;
 
    #else
 
    Dl_info info;
    struct stat buf;
 
    if(!dladdr(pAddr, &info))
        return false;
 
    if(!info.dli_fbase || !info.dli_fname)
        return false;
 
    if(stat(info.dli_fname, &buf) != 0)
        return false;
 
    base_addr = (unsigned char*)info.dli_fbase;
    base_len = buf.st_size;
    #endif
 
    return true;
}
 
/* Scan for the signature in memory then return the starting position's address */
void* CSigScan::FindSignature(void) noexcept {
    const unsigned char *pBasePtr = base_addr;
    const unsigned char *pEndPtr = base_addr+base_len;
    size_t i = 0;
 
    while(pBasePtr < pEndPtr) {
        for(i = 0;i < sig_len;i++) {
            if((sig_mask[i] != '?') && (sig_str[i] != pBasePtr[i]))
                break;
        }
 
        // If 'i' reached the end, we know we have a match!
        if(i == sig_len)
            return (void*)pBasePtr;
 
        pBasePtr++;
    }
 
    return nullptr;
}
