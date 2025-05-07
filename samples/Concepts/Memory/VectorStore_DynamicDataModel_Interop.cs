// Copyright (c) IdeaTech. All rights reserved.

using System.Text.Json;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace Memory;

[TestClass]
public class VectorStore_DynamicDataModel_Interop : BaseTest
{
    private static readonly JsonSerializerOptions indentedSerializerOptions = new() { WriteIndented = true };

    private static readonly VectorStoreRecordDefinition vectorStoreRecordDefinition = new()
    {
        Properties =
        [
            //new VectorStoreRecordKeyProperty("Key",typeof(ulong)),
            new VectorStoreRecordKeyProperty("Key",typeof(Guid)),
            new VectorStoreRecordDataProperty("Term",typeof(string)),
            new VectorStoreRecordDataProperty("Definition",typeof(string)),
            new VectorStoreRecordVectorProperty("DefinitionEmbedding",typeof(ReadOnlyMemory<float>),1536)
        ]
    };

    [TestMethod]
    public async Task UpsertWithDynamicRetrieveWithCustomAsync()
    {
        AzureOpenAITextEmbeddingGenerationService textEmbeddingGenerationService = new(
            deploymentName: TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
            apiKey: TestConfiguration.AzureOpenAIEmbeddings.ApiKey);

        // Initiate the docker container and construct the vector store.
        using QdrantClient qdrantClient = new(new Uri(TestConfiguration.Qdrant.Endpoint));
        var vectorStore = new QdrantVectorStore(qdrantClient);

        // Get and create collection if it doesn't exist using the dynamic data model and record definition that defines the schema.
        var dynamicDataModelCollection = vectorStore.GetCollection<object, Dictionary<string, object?>>("skglossary", vectorStoreRecordDefinition);
        await dynamicDataModelCollection.CreateCollectionIfNotExistsAsync();

        // Create glossary entries and generate embeddings for them.
        var glossaryEntries = CreateDynamicGlossaryEntries().ToList();
        var tasks = glossaryEntries.Select(entry => Task.Run(async () =>
        {
            entry["DefinitionEmbedding"] = await textEmbeddingGenerationService.GenerateEmbeddingAsync((string)entry["Definition"]!);
        }));
        await Task.WhenAll(tasks);

        // Upsert the glossary entries into the collection and return their keys.
        var upsertedKeysTasks = glossaryEntries.Select(x => dynamicDataModelCollection.UpsertAsync(x));
        var upsertedKeys = await Task.WhenAll(upsertedKeysTasks);

        // Retrieve one of the upserted records from the collection.
        var upsertedRecord = await dynamicDataModelCollection.GetAsync(upsertedKeys.First(), new() { IncludeVectors = true });

        // Write upserted keys and one of the upserted records to the console.
        Console.WriteLine($"Upserted keys: {string.Join(", ", upsertedKeys)}");
        //Console.WriteLine($"Upserted record: {JsonSerializer.Serialize(upsertedRecord, indentedSerializerOptions)}");

        string search = "generating a response";

        ReadOnlyMemory<float> searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(search);

        var resultRecords = await dynamicDataModelCollection.SearchEmbeddingAsync(searchVector, top: 1).ToListAsync();

        Console.WriteLine("Result: " + resultRecords[0].Record["Definition"]);
        Console.WriteLine("Score: " + resultRecords[0].Score);
    }


    /// <summary>
    /// Create some sample glossary entries using dynamic data modeling.
    /// </summary>
    /// <returns>A list of sample glossary entries.</returns>
    private static IEnumerable<Dictionary<string, object?>> CreateDynamicGlossaryEntries()
    {
        yield return new Dictionary<string, object?>
        {
            //["Key"] = 1ul,
            ["Key"] = Guid.NewGuid(),
            ["Term"] = "API",
            ["Definition"] = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
        };

        yield return new Dictionary<string, object?>
        {
            //["Key"] = 2ul,
            ["Key"] = Guid.NewGuid(),
            ["Term"] = "Connectors",
            ["Definition"] = "Connectors allow you to integrate with various services provide AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
        };

        yield return new Dictionary<string, object?>
        {
            //["Key"] = 3ul,
            ["Key"] = Guid.NewGuid(),
            ["Term"] = "RAG",
            ["Definition"] = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user’s question (prompt)."
        };
    }
}
