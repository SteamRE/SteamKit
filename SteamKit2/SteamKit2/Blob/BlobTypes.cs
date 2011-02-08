/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */



using System;

namespace SteamKit2
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
