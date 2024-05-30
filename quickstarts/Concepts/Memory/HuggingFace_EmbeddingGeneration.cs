using Newtonsoft.Json.Linq;

namespace Memory;

public class HuggingFace_EmbeddingGeneration(ITestOutputHelper output) : BaseTest(output)
{
    [RetryFact(typeof(HttpOperationException), Skip = "Microsoft.SemanticKernel.KernelException : Unexpected response from model")]
    public async Task RunInferenceApiEmbeddingAsync()
    {
        Console.WriteLine("\n======= Hugging Face Inference API - Embedding Example ========\n");

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextEmbeddingGeneration(
                model: "sentence-transformers/all-MiniLM-L6-v2",//TestConfiguration.HuggingFace.EmbeddingModelId,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        ITextEmbeddingGenerationService embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        IList<ReadOnlyMemory<float>> embeddings = await embeddingGenerator.GenerateEmbeddingsAsync(["John: Hello, how are you?\nRoger: Hey, I'm Roger!"]);

        Console.WriteLine($"Generated {embeddings.Count} embeddings for the provided text");
    }

    [Fact]
    public async Task GetEmbeddingAsync()
    {
        string texts = "Your input texts here";

        ReadOnlyMemory<float> result = await GetEmbeddingFromHuggingFaceAsync("sentence-transformers/all-MiniLM-L6-v2", texts);

        Console.WriteLine(result);
    }

    private static async Task<ReadOnlyMemory<float>> GetEmbeddingFromHuggingFaceAsync(string modelId, string texts)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestConfiguration.HuggingFace.ApiKey);

            var content = new JObject
            {
                ["inputs"] = texts,
                ["options"] = new JObject
                {
                    ["wait_for_model"] = true
                }
            };

            var response = await client.PostAsync($"https://api-inference.huggingface.co/pipeline/feature-extraction/{modelId}", new StringContent(content.ToString(), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            List<float>? embeddings = JsonSerializer.Deserialize<List<float>>(jsonResponse);

            return new ReadOnlyMemory<float>(embeddings?.ToArray());
        }
    }
}