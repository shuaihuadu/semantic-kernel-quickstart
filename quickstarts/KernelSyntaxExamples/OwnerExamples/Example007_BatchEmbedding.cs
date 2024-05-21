namespace KernelSyntaxExamples.OwnerExamples;

public class Example007_BatchEmbedding(ITestOutputHelper output) : BaseTest(output)
{
    static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(10);

    [Fact]
    public async Task RunAsync()
    {
        List<string> contents = [];

        for (int i = 0; i < 10000; i++)
        {
            contents.Add($"content{i}");
        }

        ISemanticTextMemory semanticTextMemory = new MemoryBuilder().WithAzureOpenAITextEmbeddingGeneration(
                endpoint: TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
                deploymentName: TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
                apiKey: TestConfiguration.AzureOpenAIEmbeddings.ApiKey)
            .WithMemoryStore(new VolatileMemoryStore())
            .Build();

        await SaveToMemoryStoreAsync("collection1", contents, semanticTextMemory);
    }

    private async Task SaveToMemoryStoreAsync(string collectionName, List<string> contents, ISemanticTextMemory semanticTextMemory)
    {
        List<Task> tasks = [];

        foreach (string content in contents)
        {
            tasks.Add(this.InnerSaveInformationAsync(collectionName, content, semanticTextMemory));
        }

        await Task.WhenAll(tasks);
    }

    private async Task InnerSaveInformationAsync(string collectionName, string content, ISemanticTextMemory semanticTextMemory)
    {
        await semaphoreSlim.WaitAsync();

        try
        {
            await semanticTextMemory.SaveInformationAsync(collectionName, content, content);

            WriteLine(content + " Completed!");
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}
