public sealed class TestConfiguration
{
    private readonly IConfigurationRoot _configurationRoot;

    private static TestConfiguration? _instance;

    private TestConfiguration(IConfigurationRoot configurationRoot)
    {
        this._configurationRoot = configurationRoot;
    }

    public static void Initialize(IConfigurationRoot configurationRoot)
    {
        _instance = new TestConfiguration(configurationRoot);
    }

    public static AzureOpenAIConfig AzureOpenAI => LoadSection<AzureOpenAIConfig>();

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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

    public class AzureOpenAIConfig
    {
        public string DeploymentName { get; set; }
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
    }

    public class AzureOpenAIEmbeddingsConfig
    {
        public string DeploymentName { get; set; }
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
}
