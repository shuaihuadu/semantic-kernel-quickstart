namespace Connectors.Memory.Milvus;

public interface IMilvusDbClient
{
    /// <summary>
    /// Check if a vector collection exists.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of vectors.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all vector collections.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    IAsyncEnumerable<string> ListCollectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a Milvus vector collection.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of vectors.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a vector collection.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of vectors.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific vectors by unique identifier.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of vectors.</param>
    /// <param name="keys">The unique IDs.</param>
    /// <param name="withEmbeddings">Whether to include the vector data in the returned result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The Qdrant vector record associated with the given ID if found, null if not.</returns>
    Task<IReadOnlyList<MemoryRecord>> GetFiledDataByIdsAsync(string collectionName, IEnumerable<string> keys, bool withEmbeddings, CancellationToken cancellationToken);

    /// <summary>
    /// Upsert a group of vectors into a collection.
    /// </summary>
    /// <param name="collectionName">The name assigned to a collection of vectors.</param>
    /// <param name="records">The vector records to upsert.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    Task<IReadOnlyList<string>> UpsertVectorsAsync(string collectionName, IEnumerable<MemoryRecord> records, CancellationToken cancellationToken = default);
}
