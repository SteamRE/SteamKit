#ifndef SIGNSCAN_H
#define SIGNSCAN_H

namespace NetHook
{

bool FindSignature(const char *base, const char *end, const char *sig, const char *mask, void **func, const char* prev) noexcept;

}

#endif // SIGNSCAN_H
