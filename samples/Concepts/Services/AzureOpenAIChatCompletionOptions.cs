namespace MCS.Library.AI.AzureOpenAI.Options;

public class AzureOpenAIChatCompletionOptions
{
    public const string DefaultGroupName = "Default";

    public List<AzureOpenAIOptions> Services { get; set; } = [];

    public List<AzureOpenAIOptions> EnabledServices => Services.FindAll(x => x.Enabled);

    public AzureOpenAIOptions GetEnabledService(string groupName = DefaultGroupName)
    {
        return AzureOpenAIOptions.RandomGetEnabledService(this.Services, groupName);
    }
}
