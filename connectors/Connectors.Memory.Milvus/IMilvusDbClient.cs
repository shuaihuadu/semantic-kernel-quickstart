namespace Microsoft.SemanticKernel.Connectors.Memory.Milvus;

public interface IMilvusDbClient
{
    /// <summary>
    /// Check if a collection exists.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of entities.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all collections.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    IAsyncEnumerable<string> ListCollectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a Milvus collection.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of entities.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a Milvus collection.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of entities.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific entities by unique identifier.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of entities.</param>
    /// <param name="ids">The unique IDs.</param>
    /// <param name="withEmbeddings">Whether to include the vector data in the returned result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The Milvus entity associated with the given ID if found, null if not.</returns>
    Task<IReadOnlyList<MemoryRecord>> GetFieldDataByIdsAsync(string collectionName, IEnumerable<string> ids, bool withEmbeddings, CancellationToken cancellationToken);

    /// <summary>
    /// Upsert a group of entities into a collection.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of entities.</param>
    /// <param name="records">The entities to upsert.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task<IReadOnlyList<string>> UpsertEntitiesAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entities by its unique identifier.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of entities.</param>
    /// <param name="ids">The unique IDs.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task DeleteEntitiesByIdsAsync(string collectionName, IEnumerable<string> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find the nearest vectors in a collection using vector similarity search.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of vectors.</param>
    /// <param name="target">The vector to compare the collection's vectors with.</param>
    /// <param name="minRelevanceScore">The minimum relevance score for returned results.</param>
    /// <param name="limit">The maximum number of similarity results to return.</param>
    /// <param name="withEmbeddings">Whether to include the vector data in the returned results.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task<IReadOnlyList<(MemoryRecord, double)>> FindNearestInCollectionAsync(string collectionName, ReadOnlyMemory<float> target, double minRelevanceScore, int limit = 1, bool withEmbeddings = false, CancellationToken cancellationToken = default);
}
