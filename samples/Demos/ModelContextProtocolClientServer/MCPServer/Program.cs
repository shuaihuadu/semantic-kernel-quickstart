﻿// Copyright (c) IdeaTech. All rights reserved.

using MCPServer;
using MCPServer.ProjectResources;
using MCPServer.Prompts;
using MCPServer.Resources;
using MCPServer.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Register the kernel
IKernelBuilder kernelBuilder = builder.Services.AddKernel();

// Register SK plugins
kernelBuilder.Plugins.AddFromType<DateTimeUtils>();
kernelBuilder.Plugins.AddFromType<WeatherUtils>();
kernelBuilder.Plugins.AddFromType<MailboxUtils>();

kernelBuilder.Plugins.AddFromFunctions("Agents", [AgentKernelFunctionFactory.CreateFromAgent(CreateSalesAssistantAgent())]);

// Register embedding generation service and in-memory vector store
(string Endpoint, string DeploymentName, string ApiKey) = GetConfiguration();

kernelBuilder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
kernelBuilder.Services.AddAzureOpenAITextEmbeddingGeneration(DeploymentName, Endpoint, ApiKey);

// Register MCP server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()

    // Add all functions from the kernel plugins to the MCP server as tools
    .WithTools()

    // Register the `getCurrentWeatherForCity` prompt
    .WithPrompt(PromptDefinition.Create(EmbeddedResource.ReadAsString("getCurrentWeatherForCity.json")))

    // Register vector search as MCP resource template
    .WithResourceTemplate(CreateVectorStoreSearchResourceTemplate())

    // Register the cat image as a MCP resource
    .WithResource(ResourceDefinition.CreateBlobResource(
        uri: "image://cat.jpg",
        name: "cat-image",
        content: EmbeddedResource.ReadAsBytes("cat.jpg"),
        mimeType: "image/jpeg"));

await builder.Build().RunAsync();

static (string Endpint, string deploymentName, string ApiKey) GetConfiguration()
{
    IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
        .Build();

    string endpoint = config["AzureOpenAIEmbeddings:Endpoint"] ?? throw new ArgumentNullException("endpoint");

    string deploymentName = config["AzureOpenAIEmbeddings:DeploymentName"] ?? throw new ArgumentNullException("deploymentName");

    string apiKey = config["AzureOpenAIEmbeddings:ApiKey"] ?? throw new ArgumentNullException("apiKey");

    return (endpoint, deploymentName, apiKey);
}

static ResourceTemplateDefinition CreateVectorStoreSearchResourceTemplate(Kernel? kernel = null)
{
    return new ResourceTemplateDefinition
    {
        Kernel = kernel,
        ResourceTemplate = new()
        {
            UriTemplate = "vectorStore://{collection}/{prompt}",
            Name = "Vector Store Record Retrieval",
            Description = "Retrieves relevant records from the vector store based on the provided prompt."
        },
        Handler = async (
            RequestContext<ReadResourceRequestParams> context,
            string collection,
            string prompt,
            [FromKernelServices] ITextEmbeddingGenerationService embeddingGenerationService,
            [FromKernelServices] IVectorStore vectorStore,
            CancellationToken cancellationToken) =>
        {
            // Get the vector store collection
            IVectorStoreRecordCollection<Guid, TextDataModel> vsCollection = vectorStore.GetCollection<Guid, TextDataModel>(collection);

            // Check if the collection exists, if not create and populate it
            if (!await vsCollection.CollectionExistsAsync(cancellationToken))
            {
                static TextDataModel CreateRecord(string text, ReadOnlyMemory<float> embedding)
                {
                    return new()
                    {
                        Key = Guid.NewGuid(),
                        Text = text,
                        Embedding = embedding
                    };
                }

                string content = EmbeddedResource.ReadAsString("semantic-kernel-info.txt");

                // Create a collection from the lines in the file
                await vectorStore.CreateCollectionFromListAsync<Guid, TextDataModel>(collection, content.Split('\n'), embeddingGenerationService, CreateRecord);
            }

            // Generate embedding for the prompt
            ReadOnlyMemory<float> promptEmbedding = await embeddingGenerationService.GenerateEmbeddingAsync(prompt, cancellationToken: cancellationToken);

            // Retrieve top three matching records from the vector store
            IAsyncEnumerable<VectorSearchResult<TextDataModel>> result = vsCollection.SearchEmbeddingAsync(promptEmbedding, top: 3, cancellationToken: cancellationToken);

            // Return the records as resource contents
            List<ResourceContents> contents = [];

            await foreach (var record in result)
            {
                contents.Add(new TextResourceContents()
                {
                    Text = record.Record.Text,
                    Uri = context.Params!.Uri!,
                    MimeType = "text/plain",
                });
            }

            return new ReadResourceResult { Contents = contents };
        }
    };
}

static Agent CreateSalesAssistantAgent()
{
    IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.Plugins.AddFromType<OrderProcessingUtils>();

    (string Endpoint, string DeploymentName, string ApiKey) = GetConfiguration();

    kernelBuilder.Services.AddAzureOpenAIChatCompletion(deploymentName: DeploymentName, endpoint: Endpoint, apiKey: ApiKey);

    Kernel kernel = kernelBuilder.Build();

    return new ChatCompletionAgent
    {
        Name = "SalesAssistant",
        Instructions = "You are a sales assistant. Place orders for items the user requests and handle refunds.",
        Description = "Agent to invoke to place orders for items the user requests and handle refunds.",
        Kernel = kernel,
        Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    };
}
