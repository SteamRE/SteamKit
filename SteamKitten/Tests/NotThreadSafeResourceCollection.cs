using Xunit;

namespace Tests;

[CollectionDefinition( nameof( NotThreadSafeResourceCollection ), DisableParallelization = true )]
public class NotThreadSafeResourceCollection
{
    // DebugLog is not thread-safe.
}
