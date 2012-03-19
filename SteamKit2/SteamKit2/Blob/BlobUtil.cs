using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SteamKit2.Blob
{
    enum SuggestedType
    {
        StringType,
        Int8Type,
        Int16Type,
        Int32Type,
        Int64Type,
        ByteArrayType
    }

    static class BlobUtil
    {
        public static bool IsIntDescriptor(byte[] input)
        {
            if (IsPrintableASCIIString(input))
                return false;

            return input.Length == 4;
        }

        public static SuggestedType GetSuggestedType(byte[] input)
        {
            if (IsPrintableASCIIString(input))
                return SuggestedType.StringType;

            switch (input.Length)
            {
                case 1:
                    return SuggestedType.Int8Type;
                case 2:
                    return SuggestedType.Int16Type;
                case 4:
                    return SuggestedType.Int32Type;
                case 8:
                    return SuggestedType.Int64Type;
            }

            return SuggestedType.ByteArrayType;
        }

        public static bool IsPrintableASCIIString(byte[] data)
        {
            if (data == null || data.Length == 1)
                return false;

            for (int i = 0; i < data.Length - 1; i++)
            {
                if ((data[i] < 32 || data[i] > 127))
                    return false;
            }

            return true;
        }

        public static string TrimNull(string input)
        {
            return input.TrimEnd(new char[] { '\0' } );
        }

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
    }
}
