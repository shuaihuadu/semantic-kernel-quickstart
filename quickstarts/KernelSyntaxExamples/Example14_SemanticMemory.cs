namespace KernelSyntaxExamples;

public static class Example14_SemanticMemory
{
#pragma warning disable SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private const string MemoryCollectionName = "SKGitHub";

    public static async Task RunAsync()
    {
        Console.WriteLine("==============================================================");
        Console.WriteLine("======== Semantic Memory using Azure AI Search ========");
        Console.WriteLine("==============================================================");

#pragma warning disable SKEXP0021 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        ISemanticTextMemory memoryWithACS = new MemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration(TestConfiguration.AzureOpenAIEmbeddings.DeploymentName, TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ApiKey)
            .WithMemoryStore(new AzureAISearchMemoryStore(TestConfiguration.AzureAISearch.Endpoint, TestConfiguration.AzureAISearch.ApiKey))
            .Build();
#pragma warning restore SKEXP0011 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0021 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        await RunExampleAsync(memoryWithACS);
    }

    public static async Task RunExampleAsync(ISemanticTextMemory memory)
    {
        await StoreMemoryAsync(memory);

        await SearchMemoryAsync(memory, "How do I get started?");

        await SearchMemoryAsync(memory, "Can I build a chat with SK?");
    }

    private static async Task SearchMemoryAsync(ISemanticTextMemory memory, string query)
    {
        Console.WriteLine("\nQuery: " + query + "\n");

        IAsyncEnumerable<MemoryQueryResult> memoryQueryResults = memory.SearchAsync(MemoryCollectionName, query, limit: 2, minRelevanceScore: 0.5);

        int i = 0;

        await foreach (MemoryQueryResult memoryQueryResult in memoryQueryResults)
        {
            Console.WriteLine($"Result {++i}:");
            Console.WriteLine("  URL:     : " + memoryQueryResult.Metadata.Id);
            Console.WriteLine("  Title    : " + memoryQueryResult.Metadata.Description);
            Console.WriteLine("  Relevance: " + memoryQueryResult.Relevance);
            Console.WriteLine();
        };
    }

    private static async Task StoreMemoryAsync(ISemanticTextMemory memory)
    {
        Console.WriteLine("\nAdding some GitHub file URLs and their descriptions to the semantic memory.");

        Dictionary<string, string> githubFiles = SampleData();

        int i = 0;

        foreach (var githubFile in githubFiles)
        {
            await memory.SaveReferenceAsync(
                collection: MemoryCollectionName,
                externalSourceName: "Github",
                externalId: githubFile.Key,
                description: githubFile.Value,
                text: githubFile.Value);

            Console.Write($" #{++i} saved.");
        }

        Console.WriteLine("\n----------------------");
    }

#pragma warning restore SKEXP0003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    private static Dictionary<string, string> SampleData()
    {
        return new Dictionary<string, string>
        {
            ["https://github.com/microsoft/semantic-kernel/blob/main/README.md"]
                = "README: Installation, getting started, and how to contribute",
            ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/02-running-prompts-from-file.ipynb"]
                = "Jupyter notebook describing how to pass prompts from a file to a semantic plugin or function",
            ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks//00-getting-started.ipynb"]
                = "Jupyter notebook describing how to get started with the Semantic Kernel",
            ["https://github.com/microsoft/semantic-kernel/tree/main/samples/plugins/ChatPlugin/ChatGPT"]
                = "Sample demonstrating how to create a chat plugin interfacing with ChatGPT",
            ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/SemanticKernel/Memory/VolatileMemoryStore.cs"]
                = "C# class that defines a volatile embedding store",
        };
    }
}
