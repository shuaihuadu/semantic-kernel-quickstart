namespace Caching;

public class SemanticCachingWithFilters(ITestOutputHelper output) : BaseTest(output)
{
    private const double SimilarityScore = 0.9;

    [Fact]
    public async Task InMemoryCacheAsync()
    {
        Kernel kernel = GetKernelWithCache(_ => new VolatileMemoryStore());

        FunctionResult result1 = await ExecuteAsync(kernel, "First run", "What's the tallest building in New York?");
        FunctionResult result2 = await ExecuteAsync(kernel, "Second run", "What is the highest building in New York City?");

        Console.WriteLine($"Result 1:{result1}");
        Console.WriteLine($"Result 2:{result2}");
    }

    private async Task<FunctionResult> ExecuteAsync(Kernel kernel, string title, string prompt)
    {
        Console.WriteLine($"{title}: {prompt}");

        Stopwatch stopwatch = Stopwatch.StartNew();

        FunctionResult result = await kernel.InvokePromptAsync(prompt);

        stopwatch.Stop();

        Console.WriteLine($@"Elapsed Time: {stopwatch.Elapsed:hh\:mm\:ss\.FFF}");

        return result;
    }

    private Kernel GetKernelWithCache(Func<IServiceProvider, IMemoryStore> cacheFactory)
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.AddAzureOpenAITextEmbeddingGeneration(
            deploymentName: TestConfiguration.AzureOpenAIEmbeddings.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAIEmbeddings.Endpoint,
            apiKey: TestConfiguration.AzureOpenAIEmbeddings.ApiKey);

        builder.Services.AddSingleton<IMemoryStore>(cacheFactory);

        builder.Services.AddSingleton<ISemanticTextMemory, SemanticTextMemory>();

        builder.Services.AddSingleton<IPromptRenderFilter, PromptRenderFilter>();

        builder.Services.AddSingleton<IFunctionInvocationFilter, FunctionCacheFilter>();

        return builder.Build();
    }

    public class CacheBaseFilter
    {
        protected const string CollectionName = "llm_response";

        protected const string RecordIdKey = "CacheRecordId";
    }

    public sealed class PromptRenderFilter(ISemanticTextMemory semanticTextMemory) : CacheBaseFilter, IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            await next(context);

            string prompt = context.RenderedPrompt!;

            MemoryQueryResult? searchResult = await semanticTextMemory.SearchAsync(CollectionName, prompt, limit: 1, minRelevanceScore: SimilarityScore).FirstOrDefaultAsync();

            if (searchResult is not null)
            {
                context.Result = new FunctionResult(context.Function, searchResult.Metadata.AdditionalMetadata)
                {
                    Metadata = new Dictionary<string, object?>
                    {
                        [RecordIdKey] = searchResult.Metadata.Id
                    }
                };
            }
        }
    }

    public sealed class FunctionCacheFilter(ISemanticTextMemory semanticTextMemory) : CacheBaseFilter, IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            await next(context);

            FunctionResult result = context.Result;

            if (!string.IsNullOrEmpty(context.Result.RenderedPrompt))
            {
                string? recordId = context.Result.Metadata?.GetValueOrDefault(RecordIdKey, Guid.NewGuid().ToString()) as string;

                await semanticTextMemory.SaveInformationAsync(CollectionName, context.Result.RenderedPrompt, recordId!, additionalMetadata: result.ToString());
            }
        }
    }
}
