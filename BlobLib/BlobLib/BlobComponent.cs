using System;
using System.Text;
using System.Collections.Generic;

namespace BlobLib
{
    public class BlobComponent
    {
        public BlobComponent parent;

        public virtual void Dump(StringBuilder sb, int level)
        {
        }

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
}
