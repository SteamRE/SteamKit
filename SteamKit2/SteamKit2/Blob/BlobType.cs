
namespace SteamKit2.Blob
{
   /// <summary>
    /// Gets the preprocess code of the blob.
    /// </summary>
    public enum EAutoPreprocessCode
    {
        /// <summary>
        /// The blob data was not preprocessed.
        /// </summary>
        eAutoPreprocessCodePlaintext = 80, // 'P' = Plaintext
        /// <summary>
        /// The blob data was compressed.
        /// </summary>
        eAutoPreprocessCodeCompressed = 67, // 'C' = Compressed
        /// <summary>
        /// The blob data was encrypted.
        /// </summary>
        eAutoPreprocessCodeEncrypted = 69, // 'E' = Encrypted
    };

    /// <summary>
    /// Gets the cache state of the blob.
    /// </summary>
    public enum ECacheState
    {
        /// <summary>
        /// The cache is empty.
        /// </summary>
        eCacheEmpty = 0,
        /// <summary>
        /// The cache is a preprocessed version.
        /// </summary>
        eCachedMallocedPreprocessedVersion = 1,
        /// <summary>
        /// The cache is a plaintext version.
        /// </summary>
        eCachedMallocedPlaintextVersion = 2,
        /// <summary>
        /// The cache is a preprocessed version.
        /// </summary>
        eCachePtrIsCopyOnWritePreprocessedVersion = 3,
        /// <summary>
        /// The cache is a plaintext version.
        /// </summary>
        eCachePtrIsCopyOnWritePlaintextVersion = 4
    };
}