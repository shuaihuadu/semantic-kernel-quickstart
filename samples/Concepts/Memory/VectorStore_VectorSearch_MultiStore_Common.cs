// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.Extensions.VectorData;

namespace Memory;

public class VectorStore_VectorSearch_MultiStore_Common(IVectorStore vectorStore, ITextEmbeddingGenerationService textEmbeddingGenerationService)
{
    private readonly IVectorStore _vectorStore = vectorStore;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService = textEmbeddingGenerationService;

    public async Task IngestDataAndSearchAsync<TKey>(string collectionName, Func<TKey> uniqueKeyGenerator)
        where TKey : notnull
    {
        IVectorStoreRecordCollection<TKey, Glossary<TKey>> collection = this._vectorStore.GetCollection<TKey, Glossary<TKey>>(collectionName);
        await collection.CreateCollectionIfNotExistsAsync();

        List<Glossary<TKey>> glossaryEntries = [.. CreateGlossaryEntries<TKey>(uniqueKeyGenerator)];

        IEnumerable<Task> tasks = glossaryEntries.Select(entry => Task.Run(async () =>
        {
            entry.DefinitionEmbedding = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(entry.Definition);
        }));

        await Task.WhenAll(tasks);

        IEnumerable<Task<TKey>> upsertedKeysTasks = glossaryEntries.Select(x => collection.UpsertAsync(x));
        TKey[]? upsertedKeys = await Task.WhenAll(upsertedKeysTasks);

        string searchString = "What is an Application Programming Interface";
        ReadOnlyMemory<float> searchVector = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(searchString);
        List<VectorSearchResult<Glossary<TKey>>> resultRecords = await collection.SearchEmbeddingAsync(searchVector, top: 1).ToListAsync();

        Console.WriteLine("Search string: " + searchString);
        Console.WriteLine("Result: " + resultRecords.First().Record.Definition);
        Console.WriteLine();

        searchString = "What is Retrieval Augmented Generation";
        searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(searchString);
        resultRecords = await collection.SearchEmbeddingAsync(searchVector, top: 1).ToListAsync();

        Console.WriteLine("Search string: " + searchString);
        Console.WriteLine("Result: " + resultRecords.First().Record.Definition);
        Console.WriteLine();


        searchString = "What is Retrieval Augmented Generation";
        searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(searchString);
        resultRecords = await collection.SearchEmbeddingAsync(searchVector, top: 3, new() { Filter = g => g.Category == "External Definitions" }).ToListAsync();


        Console.WriteLine("Search string: " + searchString);
        Console.WriteLine("Number of results: " + resultRecords.Count);
        Console.WriteLine("Result 1 Score: " + resultRecords[0].Score);
        Console.WriteLine("Result 1: " + resultRecords[0].Record.Definition);
        Console.WriteLine("Result 2 Score: " + resultRecords[1].Score);
        Console.WriteLine("Result 2: " + resultRecords[1].Record.Definition);
    }



    /// <summary>
    /// Create some sample glossary entries.
    /// </summary>
    /// <typeparam name="TKey">The type of the model key.</typeparam>
    /// <param name="uniqueKeyGenerator">A function that can be used to generate unique keys for the model in the type that the model requires.</param>
    /// <returns>A list of sample glossary entries.</returns>
    private static IEnumerable<Glossary<TKey>> CreateGlossaryEntries<TKey>(Func<TKey> uniqueKeyGenerator)
    {
        yield return new Glossary<TKey>
        {
            Key = uniqueKeyGenerator(),
            Category = "External Definitions",
            Term = "API",
            Definition = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
        };

        yield return new Glossary<TKey>
        {
            Key = uniqueKeyGenerator(),
            Category = "Core Definitions",
            Term = "Connectors",
            Definition = "Connectors allow you to integrate with various services provide AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
        };

        yield return new Glossary<TKey>
        {
            Key = uniqueKeyGenerator(),
            Category = "External Definitions",
            Term = "RAG",
            Definition = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user’s question (prompt)."
        };
    }

    /// <summary>
    /// Sample model class that represents a glossary entry.
    /// </summary>
    /// <remarks>
    /// Note that each property is decorated with an attribute that specifies how the property should be treated by the vector store.
    /// This allows us to create a collection in the vector store and upsert and retrieve instances of this class without any further configuration.
    /// </remarks>
    /// <typeparam name="TKey">The type of the model key.</typeparam>
    private sealed class Glossary<TKey>
    {
        [VectorStoreRecordKey]
        public TKey Key { get; set; }

        [VectorStoreRecordData(IsIndexed = true)]
        public string Category { get; set; }

        [VectorStoreRecordData]
        public string Term { get; set; }

        [VectorStoreRecordData]
        public string Definition { get; set; }

        [VectorStoreRecordVector(1536)]
        public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
    }
}
