using Microsoft.SemanticKernel.Embeddings;

namespace MCS.Library.AI.AzureOpenAI;

public class TextEmbeddingService(ILoggerFactory loggerFactory, Kernel kernel)
{
    private readonly ILogger<TextEmbeddingService> _logger = loggerFactory.CreateLogger<TextEmbeddingService>();
    private readonly Kernel _kernel = kernel;

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string content)
    {
        ITextEmbeddingGenerationService textEmbeddingGenerationService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        this._logger.LogInformation("获取内容[{Content}]的Embedding", content);

        return await textEmbeddingGenerationService.GenerateEmbeddingAsync(content);
    }
}
