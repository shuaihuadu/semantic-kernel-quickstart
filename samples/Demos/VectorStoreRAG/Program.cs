// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using VectorStoreRAG;
using VectorStoreRAG.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true);

builder.Services.Configure<RagConfig>(builder.Configuration.GetSection(RagConfig.ConfigSectionName));

ApplicationConfig applicationConfig = new(builder.Configuration);

using CancellationTokenSource cancellationTokenSource = new();
CancellationToken cancellationToken = cancellationTokenSource.Token;
builder.Services.AddKeyedSingleton("AppShutdown", cancellationTokenSource);

IKernelBuilder kernelBuilder = builder.Services.AddKernel();

kernelBuilder.AddAzureOpenAIChatCompletion(
    applicationConfig.AzureOpenAIConfig.DeploymentName,
    applicationConfig.AzureOpenAIConfig.Endpoint,
    applicationConfig.AzureOpenAIConfig.ApiKey);

kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
    applicationConfig.AzureOpenAIEmbeddingsConfig.DeploymentName,
    applicationConfig.AzureOpenAIEmbeddingsConfig.Endpoint,
    applicationConfig.AzureOpenAIEmbeddingsConfig.ApiKey);

switch (applicationConfig.RagConfig.VectorStoreType)
{
    case "InMemory":
        kernelBuilder.AddInMemoryVectorStoreRecordCollection<string, TextSnippet<string>>(applicationConfig.RagConfig.CollectionName, new InMemoryVectorStoreRecordCollectionOptions<string, TextSnippet<string>>
        {
            EmbeddingGenerator = new AzureOpenAITextEmbeddingGenerationService(
                applicationConfig.AzureOpenAIEmbeddingsConfig.DeploymentName,
                applicationConfig.AzureOpenAIEmbeddingsConfig.Endpoint,
                applicationConfig.AzureOpenAIEmbeddingsConfig.ApiKey)
            .AsEmbeddingGenerator()
        });
        break;
    case "Qdrant":
        kernelBuilder.AddQdrantVectorStoreRecordCollection<Guid, TextSnippet<Guid>>(
            applicationConfig.RagConfig.CollectionName,
            applicationConfig.QdrantConfig.Host,
            applicationConfig.QdrantConfig.Port,
            applicationConfig.QdrantConfig.Https,
            applicationConfig.QdrantConfig.ApiKey);
        break;
    case "Redis":
        kernelBuilder.AddRedisJsonVectorStoreRecordCollection<TextSnippet<string>>(
            applicationConfig.RagConfig.CollectionName,
            applicationConfig.RedisConfig.ConnectionString);
        break;
    default:
        throw new NotSupportedException($"Vector store type '{applicationConfig.RagConfig.VectorStoreType}' is not supported.");
}

switch (applicationConfig.RagConfig.VectorStoreType)
{
    case "InMemory":
    case "Redis":
        RegisterServices<string>(builder, kernelBuilder, applicationConfig);
        break;
    case "Qdrant":
        RegisterServices<Guid>(builder, kernelBuilder, applicationConfig);
        break;
    default:
        throw new NotSupportedException($"Vector store type '{applicationConfig.RagConfig.VectorStoreType}' is not supported.");
}

using IHost host = builder.Build();

await host.RunAsync(cancellationToken).ConfigureAwait(false);

static void RegisterServices<TKey>(HostApplicationBuilder builder, IKernelBuilder kernelBuilder, ApplicationConfig applicationConfig)
    where TKey : notnull
{
    // Add a text search implementation that uses the registered vector store record collection for search.
    kernelBuilder.AddVectorStoreTextSearch<TextSnippet<TKey>>(
        new TextSearchStringMapper((result) => (result as TextSnippet<TKey>)!.Text!),
        new TextSearchResultMapper((result) =>
        {
            // Create a mapping from the Vector Store data type to the data type returned by the Text Search.
            // This text search will ultimately be used in a plugin and this TextSearchResult will be returned to the prompt template
            // when the plugin is invoked from the prompt template.
            var castResult = result as TextSnippet<TKey>;
            return new TextSearchResult(value: castResult!.Text!) { Name = castResult.ReferenceDescription, Link = castResult.ReferenceLink };
        }));

    builder.Services.AddSingleton<UniqueKeyGenerator<Guid>>(new UniqueKeyGenerator<Guid>(Guid.NewGuid));
    builder.Services.AddSingleton<UniqueKeyGenerator<string>>(new UniqueKeyGenerator<string>(() => Guid.NewGuid().ToString()));

    builder.Services.AddSingleton<IDataLoader, DataLoader<TKey>>();

    builder.Services.AddHostedService<RAGChatService<TKey>>();
}
