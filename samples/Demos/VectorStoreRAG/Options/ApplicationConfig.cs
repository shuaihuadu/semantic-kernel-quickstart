// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.Extensions.Configuration;

namespace VectorStoreRAG.Options;
internal sealed class ApplicationConfig
{
    private readonly AzureOpenAIConfig _azureOpenAIConfig;
    private readonly AzureOpenAIEmbeddingsConfig _azureOpenAIEmbeddingsConfig = new();
    private readonly RagConfig _ragConfig = new();
    private readonly QdrantConfig _qdrantConfig = new();
    private readonly RedisConfig _redisConfig = new();

    public ApplicationConfig(ConfigurationManager configurationManager)
    {
        this._azureOpenAIConfig = new();

        configurationManager
            .GetRequiredSection($"{AzureOpenAIConfig.ConfigSectionName}")
            .Bind(this._azureOpenAIConfig);
        configurationManager
            .GetRequiredSection($"{AzureOpenAIEmbeddingsConfig.ConfigSectionName}")
            .Bind(this._azureOpenAIEmbeddingsConfig);
        configurationManager
            .GetRequiredSection(RagConfig.ConfigSectionName)
            .Bind(this._ragConfig);
        configurationManager
            .GetRequiredSection($"{QdrantConfig.ConfigSectionName}")
            .Bind(this._qdrantConfig);
        configurationManager
            .GetRequiredSection($"{RedisConfig.ConfigSectionName}")
            .Bind(this._redisConfig);
    }

    public AzureOpenAIConfig AzureOpenAIConfig => this._azureOpenAIConfig;

    public AzureOpenAIEmbeddingsConfig AzureOpenAIEmbeddingsConfig => this._azureOpenAIEmbeddingsConfig;

    public RagConfig RagConfig => this._ragConfig;

    public QdrantConfig QdrantConfig => this._qdrantConfig;

    public RedisConfig RedisConfig => this._redisConfig;
}
