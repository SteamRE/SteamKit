
namespace SteamKit2.Blob
{
    internal static class BlobUtil
    {
        public static bool IsValidCacheState(ECacheState cachestate)
        {
            return (cachestate >= ECacheState.eCacheEmpty && cachestate <= ECacheState.eCachePtrIsCopyOnWritePlaintextVersion);
        }

        public static bool IsValidProcess(EAutoPreprocessCode process)
        {
            return (process == EAutoPreprocessCode.eAutoPreprocessCodePlaintext ||
                    process == EAutoPreprocessCode.eAutoPreprocessCodeCompressed ||
                    process == EAutoPreprocessCode.eAutoPreprocessCodeEncrypted);
        }

        // unsafe from http://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net
        public static unsafe bool UnsafeCompare(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                return true;
            }
        }
    }
}
