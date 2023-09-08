namespace Microsoft.SemanticKernel.Connectors.Memory.Milvus;

/// <summary>
/// Milvus memory record entity.
/// </summary>
public sealed class MilvusMemoryRecord
{
    /// <summary>
    /// The Milvus unique entity id.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The vector data.
    /// </summary>
    public ReadOnlyMemory<float> Embedding { get; set; }

    /// <summary>
    /// The Milvus $meta data.
    /// </summary>
    public string Meta { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvusMemoryRecord"/> class.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="embedding"></param>
    /// <param name="meta"></param>
    public MilvusMemoryRecord(string id, ReadOnlyMemory<float> embedding, string meta)
    {
        this.Id = id;
        this.Embedding = embedding;
        this.Meta = meta;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MilvusMemoryRecord"/> class.
    /// </summary>
    /// <param name="record">Instance of <see cref="MemoryRecord"/>.</param>
    public MilvusMemoryRecord(MemoryRecord record) : this(string.IsNullOrEmpty(record.Key) ? record.Metadata.Id : record.Key, record.Embedding, record.GetSerializedMetadata()) { }

    /// <summary>
    /// Convert to <see cref="MemoryRecord"/>.
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    public MemoryRecord ToMemoryRecord()
    {
        return MemoryRecord.FromJsonMetadata(
            json: this.Meta,
            embedding: this.Embedding,
            key: this.Id);
    }
}
