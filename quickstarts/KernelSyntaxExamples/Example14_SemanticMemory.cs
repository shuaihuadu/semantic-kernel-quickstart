
namespace KernelSyntaxExamples;

public class Example14_SemanticMemory : BaseTest
{
    private const string MemoryCollectionName = "SKGitHub";

    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("==============================================================");
        this.WriteLine("======== Semantic Memory using Azure AI Search ========");
        this.WriteLine("==============================================================");

        ISemanticTextMemory memoryWithACS = new MemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration(TestConfiguration.AzureOpenAIEmbeddings.DeploymentName, TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ApiKey)
            .WithMemoryStore(new AzureAISearchMemoryStore(TestConfiguration.AzureAISearch.Endpoint, TestConfiguration.AzureAISearch.ApiKey))
            .Build();

        await RunExampleAsync(memoryWithACS);

        this.WriteLine("====================================================");
        this.WriteLine("======== Semantic Memory (volatile, in RAM) ========");
        this.WriteLine("====================================================");

        ISemanticTextMemory memoryWithVolatile = new MemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration(TestConfiguration.AzureOpenAIEmbeddings.DeploymentName, TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ApiKey)
            .WithMemoryStore(new VolatileMemoryStore())
            .Build();

        await RunExampleAsync(memoryWithVolatile);
    }

    public async Task RunExampleAsync(ISemanticTextMemory memory)
    {
        await StoreMemoryAsync(memory);

        await SearchMemoryAsync(memory, "How do I get started?");

        await SearchMemoryAsync(memory, "Can I build a chat with SK?");
    }

    private async Task SearchMemoryAsync(ISemanticTextMemory memory, string query)
    {
        this.WriteLine("\nQuery: " + query + "\n");

        IAsyncEnumerable<MemoryQueryResult> memoryQueryResults = memory.SearchAsync(MemoryCollectionName, query, limit: 2, minRelevanceScore: 0.5);

        int i = 0;

        await foreach (MemoryQueryResult memoryQueryResult in memoryQueryResults)
        {
            this.WriteLine($"Result {++i}:");
            this.WriteLine("  URL:     : " + memoryQueryResult.Metadata.Id);
            this.WriteLine("  Title    : " + memoryQueryResult.Metadata.Description);
            this.WriteLine("  Relevance: " + memoryQueryResult.Relevance);
            this.WriteLine();
        };
    }

    private async Task StoreMemoryAsync(ISemanticTextMemory memory)
    {
        this.WriteLine("\nAdding some GitHub file URLs and their descriptions to the semantic memory.");

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

            this.Write($" #{++i} saved.");
        }

        this.WriteLine("\n----------------------");
    }

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

    public Example14_SemanticMemory(ITestOutputHelper output) : base(output)
    {
    }
}
