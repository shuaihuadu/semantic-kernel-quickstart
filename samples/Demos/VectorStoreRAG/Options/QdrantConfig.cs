// Copyright (c) IdeaTech. All rights reserved.

namespace VectorStoreRAG.Options;

/// <summary>
/// Qdrant service settings.
/// </summary>
internal sealed class QdrantConfig
{
    public const string ConfigSectionName = "Qdrant";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 6333;

    public bool Https { get; set; } = false;

    public string ApiKey { get; set; } = string.Empty;
}
