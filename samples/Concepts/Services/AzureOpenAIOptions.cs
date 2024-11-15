namespace MCS.Library.AI.AzureOpenAI.Options;

public class AzureOpenAIOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string DeploymentName { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string GroupName { get; set; } = string.Empty;

    public static AzureOpenAIOptions RandomGetEnabledService(IEnumerable<AzureOpenAIOptions> services, string groupName)
    {
        var enabledServices = services
            .Where(x => x.Enabled && (x.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(x.GroupName)))
            .ToList();

        if (enabledServices.Count == 0)
        {
            throw new InvalidOperationException($"没有获取到可用的GroupName为{groupName}的AzureOpenAI配置项.");
        }

        if (enabledServices.Count == 1)
        {
            return enabledServices[0];
        }

        Random random = new(Guid.NewGuid().GetHashCode());

        int randomIndex = random.Next(enabledServices.Count);

        return enabledServices[randomIndex];
    }
}
