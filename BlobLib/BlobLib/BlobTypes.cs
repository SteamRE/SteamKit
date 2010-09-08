using System;

namespace BlobLib
{
    public enum EAutoPreprocessCode
    {
        eAutoPreprocessCodePlaintext = 80, // 'P' = Plaintext
        eAutoPreprocessCodeCompressed = 67, // 'C' = Compressed
        eAutoPreprocessCodeEncrypted = 69, // 'E' = Encrypted
    };

    public enum ECacheState
    {
        eCacheEmpty = 0,
        eCachedMallocedPreprocessedVersion = 1,
        eCachedMallocedPlaintextVersion = 2,
        eCachePtrIsCopyOnWritePreprocessedVersion = 3,
        eCachePtrIsCopyOnWritePlaintextVersion = 4
    };
}
