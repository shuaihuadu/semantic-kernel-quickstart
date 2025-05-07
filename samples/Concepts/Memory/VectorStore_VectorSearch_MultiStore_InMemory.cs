// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace Memory;

[TestClass]
public class VectorStore_VectorSearch_MultiStore_InMemory : BaseTest
{
    [TestMethod]
    public async Task ExampleWithDIAsync()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
            deploymentName: TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
            apiKey: TestConfiguration.AzureOpenAIEmbeddings.ApiKey);

        kernelBuilder.AddInMemoryVectorStore();
        kernelBuilder.Services.AddTransient<VectorStore_VectorSearch_MultiStore_Common>();

        Kernel kernel = kernelBuilder.Build();

        VectorStore_VectorSearch_MultiStore_Common processor = kernel.GetRequiredService<VectorStore_VectorSearch_MultiStore_Common>();

        int uniqueId = 0;

        await processor.IngestDataAndSearchAsync("skglossaryWithDI", () => uniqueId++);
    }

    [TestMethod]
    public async Task ExampleWithoutDIAsync()
    {
        AzureOpenAITextEmbeddingGenerationService textEmbeddingGenerationService = new(
            TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
            TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
            TestConfiguration.AzureOpenAIEmbeddings.ApiKey);

        InMemoryVectorStore vectorStore = new();

        VectorStore_VectorSearch_MultiStore_Common processor = new(vectorStore, textEmbeddingGenerationService);

        int uniqueId = 0;

        await processor.IngestDataAndSearchAsync("skglossaryWithoutDI", () => uniqueId++);
    }
}
