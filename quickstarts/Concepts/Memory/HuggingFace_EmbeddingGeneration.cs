using Microsoft.SemanticKernel.Embeddings;

namespace Memory;

public class HuggingFace_EmbeddingGeneration(ITestOutputHelper output) : BaseTest(output)
{

    [RetryFact(typeof(HttpOperationException), Skip = "TODO Hugging Face ")]
    public async Task RunInferenceApiEmbeddingAsync()
    {
        this.WriteLine("\n======= Hugging Face Inference API - Embedding Example ========\n");

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextEmbeddingGeneration(
                model: TestConfiguration.HuggingFace.EmbeddingModelId,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        ITextEmbeddingGenerationService embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        IList<ReadOnlyMemory<float>> embeddings = await embeddingGenerator.GenerateEmbeddingsAsync(["John: Hello, how are you?\nRoger: Hey, I'm Roger!"]);

        WriteLine($"Generated {embeddings.Count} embeddings for the provided text");
    }
}
