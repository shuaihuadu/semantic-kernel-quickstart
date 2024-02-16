namespace KernelSyntaxExamples;

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

    public static OpenAIConfig OpenAI => LoadSection<OpenAIConfig>();
    public static AzureOpenAIConfig AzureOpenAI => LoadSection<AzureOpenAIConfig>();
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
    public static GithubConfig Github => LoadSection<GithubConfig>();
    public static PostgresConfig Postgres => LoadSection<PostgresConfig>();
    public static RedisConfig Redis => LoadSection<RedisConfig>();
    public static JiraConfig Jira => LoadSection<JiraConfig>();
    public static ChromaConfig Chroma => LoadSection<ChromaConfig>();
    public static KustoConfig Kusto => LoadSection<KustoConfig>();
    public static MongoDBConfig Mongo => LoadSection<MongoDBConfig>();
    public static MilvusConfig Milvus => LoadSection<MilvusConfig>();
    public static MsGraphConfiguration MsGraph => LoadSection<MsGraphConfiguration>();

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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    public class OpenAIConfig
    {
        public string ModelId { get; set; }
        public string ChatModelId { get; set; }
        public string EmbeddingModelId { get; set; }
        public string ApiKey { get; set; }
    }

    public class AzureOpenAIConfig
    {
        public string ServiceId { get; set; }
        public string DeploymentName { get; set; }
        public string ModelId { get; set; }
        public string ChatDeploymentName { get; set; }
        public string ChatModelId { get; set; }
        public string ImageDeploymentName { get; set; }
        public string ImageModelId { get; set; }
        public string ImageEndpoint { get; set; }
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string ImageApiKey { get; set; }
        public string VisionDeploymentName { get; set; }
        public string TTSDeploymentName { get; set; }
        public string TTSEndpoint { get; set; }
        public string TTSApiKey { get; set; }
    }

    public class AzureOpenAIEmbeddingsConfig
    {
        public string DeploymentName { get; set; }
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
    }

    public class AzureAISearchConfig
    {
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string IndexName { get; set; }
    }

    public class QdrantConfig
    {
        public string Endpoint { get; set; }
        public string Port { get; set; }
    }

    public class WeaviateConfig
    {
        public string Scheme { get; set; }
        public string Endpoint { get; set; }
        public string Port { get; set; }
        public string ApiKey { get; set; }
    }

    public class KeyVaultConfig
    {
        public string Endpoint { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class HuggingFaceConfig
    {
        public string ApiKey { get; set; }
        public string ModelId { get; set; }
    }

    public class PineconeConfig
    {
        public string ApiKey { get; set; }
        public string Environment { get; set; }
    }

    public class BingConfig
    {
        public string ApiKey { get; set; }
    }

    public class GoogleConfig
    {
        public string ApiKey { get; set; }
        public string SearchEngineId { get; set; }
    }

    public class GithubConfig
    {
        public string PAT { get; set; }
    }

    public class PostgresConfig
    {
        public string ConnectionString { get; set; }
    }

    public class RedisConfig
    {
        public string Configuration { get; set; }
    }

    public class JiraConfig
    {
        public string ApiKey { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }
    }

    public class ChromaConfig
    {
        public string Endpoint { get; set; }
    }

    public class KustoConfig
    {
        public string ConnectionString { get; set; }
    }

    public class MongoDBConfig
    {
        public string ConnectionString { get; set; }
    }

    public class MilvusConfig
    {
        public string Host { get; set; }

        public int Port { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
}
