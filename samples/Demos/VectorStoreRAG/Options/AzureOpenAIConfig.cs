// Copyright (c) IdeaTech. All rights reserved.

namespace VectorStoreRAG.Options;

/// <summary>
/// Azure OpenAI service settings.
/// </summary>
internal sealed class AzureOpenAIConfig
{
    public const string ConfigSectionName = "AzureOpenAI";

    public string DeploymentName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
