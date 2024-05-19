public sealed class TestConfiguration
{
    private readonly IConfigurationRoot _configurationRoot;

    private static TestConfiguration? _instance;

    public static void Initialize()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .AddJsonFile(@"D:\appsettings\test_configuration.json", true)
            .Build();

        Initialize(configurationRoot);
    }

    private TestConfiguration(IConfigurationRoot configurationRoot)
    {
        this._configurationRoot = configurationRoot;
    }

    private static void Initialize(IConfigurationRoot configurationRoot)
    {
        _instance = new TestConfiguration(configurationRoot);
    }

    public static OpenAIConfig OpenAI => LoadSection<OpenAIConfig>();
    public static AzureOpenAIConfig AzureOpenAI => LoadSection<AzureOpenAIConfig>();
    public static AzureOpenAIDalle3Config AzureOpenAIDalle3 => LoadSection<AzureOpenAIDalle3Config>();
    public static AzureOpenAITTSConfig AzureOpenAITTS => LoadSection<AzureOpenAITTSConfig>();
    public static AzureOpenAIWhisperConfig AzureOpenAIWhisper => LoadSection<AzureOpenAIWhisperConfig>();
    public static AzureOpenAIConfig AzureOpenAIImages => LoadSection<AzureOpenAIConfig>();
    public static AzureOpenAIEmbeddingsConfig AzureOpenAIEmbeddings => LoadSection<AzureOpenAIEmbeddingsConfig>();
    public static AzureAISearchConfig AzureAISearch => LoadSection<AzureAISearchConfig>();
    public static QdrantConfig Qdrant => LoadSection<QdrantConfig>();
    public static WeaviateConfig Weaviate => LoadSection<WeaviateConfig>();
    public static KeyVaultConfig KeyVault => LoadSection<KeyVaultConfig>();
    public static HuggingFaceConfig HuggingFace => LoadSection<HuggingFaceConfig>();
    public static PineconeConfig Pinecone => LoadSection<PineconeConfig>();
    public static BingConfig Bing => LoadSection<BingConfig>();
    public static GoogleConfig Google => LoadSection<GoogleConfig>();
    public static GoogleAIConfig GoogleAI => LoadSection<GoogleAIConfig>();
    public static GithubConfig Github => LoadSection<GithubConfig>();
    public static PostgresConfig Postgres => LoadSection<PostgresConfig>();
    public static RedisConfig Redis => LoadSection<RedisConfig>();
    public static JiraConfig Jira => LoadSection<JiraConfig>();
    public static ChromaConfig Chroma => LoadSection<ChromaConfig>();
    public static KustoConfig Kusto => LoadSection<KustoConfig>();
    public static MongoDBConfig Mongo => LoadSection<MongoDBConfig>();
    public static MilvusConfig Milvus => LoadSection<MilvusConfig>();

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

        return _instance._configurationRoot.GetSection(caller).Get<T>() ?? throw new ConfigurationNotFoundException(section: caller);
    }

    public class OpenAIConfig
    {
        public string ModelId { get; set; } = string.Empty;
        public string ChatModelId { get; set; } = string.Empty;
        public string EmbeddingModelId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class AzureOpenAIConfig
    {
        public const string ModelId = "gpt-4-32K";

        public string DeploymentName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string VisionDeploymentName { get; set; } = string.Empty;
    }

    public class AzureOpenAIDalle3Config
    {
        public const string ModelId = "dalle-3";

        public string DeploymentName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
    public class AzureOpenAITTSConfig
    {
        public string DeploymentName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
    public class AzureOpenAIWhisperConfig
    {
        public string DeploymentName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class AzureOpenAIEmbeddingsConfig
    {
        public string DeploymentName { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class AzureAISearchConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string IndexName { get; set; } = string.Empty;
    }

    public class QdrantConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
    }

    public class WeaviateConfig
    {
        public string Scheme { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class KeyVaultConfig
    {
        public string Endpoint { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public class HuggingFaceConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public string EmbeddingModelId { get; set; } = string.Empty;
    }

    public class PineconeConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }

    public class BingConfig
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    public class GoogleConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SearchEngineId { get; set; } = string.Empty;
    }

    public class GithubConfig
    {
        public string PAT { get; set; } = string.Empty;
    }

    public class PostgresConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class RedisConfig
    {
        public string Configuration { get; set; } = string.Empty;
    }

    public class JiraConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }

    public class ChromaConfig
    {
        public string Endpoint { get; set; } = string.Empty;
    }

    public class KustoConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class MongoDBConfig
    {
        public string ConnectionString { get; set; } = string.Empty;
    }

    public class MilvusConfig
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }
    }

    public class GoogleAIConfig
    {
        public string ApiKey { get; set; } = string.Empty;

        public string EmbeddingModelId { get; set; } = string.Empty;

        public GeminiConfig Gemini { get; set; } = null!;

        public class GeminiConfig
        {
            public string ModelId { get; set; } = string.Empty;
        }
    }
}
