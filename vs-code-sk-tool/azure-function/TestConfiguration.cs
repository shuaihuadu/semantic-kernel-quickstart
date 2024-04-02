using Microsoft.Extensions.Configuration;
using Models;
using System.Runtime.CompilerServices;

namespace AiPlugin;

public sealed class TestConfiguration
{
    private readonly IConfigurationRoot _configurationRoot;

    private static TestConfiguration? _instance;

    private TestConfiguration(IConfigurationRoot configurationRoot)
    {
        _configurationRoot = configurationRoot;
    }

    public static void Initialize(IConfigurationRoot configurationRoot)
    {
        _instance = new TestConfiguration(configurationRoot);
    }

    public static AzureOpenAIConfig AzureOpenAI => LoadSection<AzureOpenAIConfig>();

    public static AiPluginSettings AiPluginSettings => LoadSection<AiPluginSettings>();

    private static T LoadSection<T>([CallerMemberName] string? caller = null)
    {
        if (_instance == null)
        {
            throw new InvalidOperationException("TestConfiguration must be initialized with a call to Initialize(IConfigurationRoot) before accessing configuration values.");
        }

        if (string.IsNullOrEmpty(caller))
        {
            throw new ArgumentNullException(nameof(caller));
        }

        return _instance._configurationRoot.GetSection(caller).Get<T>() ?? throw new Exception($"The configuration section '{caller} not found.'");
    }

    public class AzureOpenAIConfig
    {
        public string DeploymentName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}