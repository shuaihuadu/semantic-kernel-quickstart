namespace MCS.Library.AI.AzureOpenAI.Options;

public class AzureOpenAIEmbeddingOptions
{
    public List<AzureOpenAIOptions> Services { get; set; } = [];

    public AzureOpenAIOptions GetEnabledService()
    {
        return AzureOpenAIOptions.RandomGetEnabledService(this.Services, string.Empty);
    }
}
