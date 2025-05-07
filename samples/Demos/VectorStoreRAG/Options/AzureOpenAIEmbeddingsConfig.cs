// Copyright (c) IdeaTech. All rights reserved.

namespace VectorStoreRAG.Options;

/// <summary>
/// Azure OpenAI Embeddings service settings.
/// </summary>
internal sealed class AzureOpenAIEmbeddingsConfig
{
    public const string ConfigSectionName = "AzureOpenAIEmbeddings";

    public string DeploymentName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
