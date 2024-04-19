
namespace KernelSyntaxExamples;

public class Example22_OpenAIPlugin_AzureKeyVault(ITestOutputHelper output) : BaseTest(output)
{
    private const string SecretName = "Foo";
    private const string SecretValue = "Bar";

    //[Fact]
    [Fact(Skip = "Setup credentials")]
    public async Task RunAsync()
    {
        OpenAIAuthenticationProvider openAIAuthenticationProvider = new(
            new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "login.microsoftonline.com",
                    new Dictionary<string, string>()
                    {
                        {"client_id",TestConfiguration.KeyVault.ClientId },
                        {"client_secret",TestConfiguration.KeyVault.ClientSecret },
                        {"grant_type","client_credentials" }
                    }
                }
            }
        );

        Kernel kernel = new();

        var openApiSpec = EmbeddedResource.Read("22-openapi.json");

        using HttpMessageHandlerStub messageHandlerStub = new(openApiSpec);
        using HttpClient httpClient = new(messageHandlerStub);

        Stream? openAIManifest = EmbeddedResource.ReadStream("22-ai-plugin.json");

        KernelPlugin plugin = await kernel.ImportPluginFromOpenAIAsync(
            "AzureKeyVaultPlugin",
            openAIManifest!,
            new OpenAIFunctionExecutionParameters
            {
                AuthCallback = openAIAuthenticationProvider.AuthenticateRequestAsync,
                HttpClient = httpClient,
                EnableDynamicPayload = true,
                ServerUrlOverride = new Uri(TestConfiguration.KeyVault.Endpoint)
            });

        await AddSecretToAzureKeyVaultAsync(kernel, plugin);

        await GetSecretFromAzureKeyVaultWithRetryAsync(kernel, plugin);
    }

    private async Task AddSecretToAzureKeyVaultAsync(Kernel kernel, KernelPlugin plugin)
    {
        KernelArguments arguments = new()
        {
            ["secret-name"] = SecretName,
            ["value"] = SecretValue,
            ["api-version"] = "7.0",
            ["enabled"] = "true"
        };

        FunctionResult functionResult = await kernel.InvokeAsync(plugin["SetSecret"], arguments);

        RestApiOperationResponse? response = functionResult.GetValue<RestApiOperationResponse>();

        this.WriteLine($"SetSecret function result: {response?.Content?.ToString()}");
    }

    private async Task GetSecretFromAzureKeyVaultWithRetryAsync(Kernel kernel, KernelPlugin plugin)
    {
        KernelArguments arguments = new()
        {
            ["secret-name"] = SecretName,
            ["api-version"] = "7.0"
        };

        FunctionResult functionResult = await kernel.InvokeAsync(plugin["GetSecret"], arguments);

        RestApiOperationResponse? response = functionResult.GetValue<RestApiOperationResponse>();

        this.WriteLine($"GetSecret function result: {response?.Content?.ToString()}");
    }
}

internal sealed class OpenAIAuthenticationProvider(Dictionary<string, Dictionary<string, string>>? oAuthValues = null,
    Dictionary<string, string>? credentials = null)
{
    private readonly Dictionary<string, Dictionary<string, string>> _oAuthValues = oAuthValues ?? [];
    private readonly Dictionary<string, string> _credentials = credentials ?? [];

    public async Task AuthenticateRequestAsync(
        HttpRequestMessage request,
        string pluginName,
        OpenAIAuthenticationConfig openAIAuthenticationConfig,
        CancellationToken cancellationToken = default)
    {
        if (openAIAuthenticationConfig.Type == OpenAIAuthenticationType.None)
        {
            return;
        }

        string scheme = string.Empty;
        string credential = string.Empty;

        if (openAIAuthenticationConfig.Type == OpenAIAuthenticationType.OAuth)
        {
            Dictionary<string, string> domainOAuthValues = this._oAuthValues[openAIAuthenticationConfig.AuthorizationUrl!.Host]
                ?? throw new KernelException("No OAuth values found for the provider authorizatioin URL.");

            Dictionary<string, string> values = new(domainOAuthValues)
            {
                { "scope",openAIAuthenticationConfig.Scope ?? string.Empty }
            };

            using HttpContent? requestContent = openAIAuthenticationConfig.AuthorizationContentType switch
            {
                "application/x-www-form-urlencoded" => new FormUrlEncodedContent(values),
                "application/json" => new StringContent(JsonSerializer.Serialize(values), Encoding.UTF8, "application/json"),
                _ => throw new KernelException($"Unsupported authorization content type: {openAIAuthenticationConfig.AuthorizationContentType}")
            };

            string url = openAIAuthenticationConfig.AuthorizationUrl.AbsoluteUri.Replace("%3CTENANT_ID%3E", TestConfiguration.KeyVault.TenantId);

            using HttpClient client = new();
            using HttpRequestMessage authRequest = new(HttpMethod.Post, url) { Content = requestContent };
            //using HttpRequestMessage authRequest = new(HttpMethod.Post, openAIAuthenticationConfig.AuthorizationUrl) { Content = requestContent };

            HttpResponseMessage response = await client.SendAsync(authRequest, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            OAuthTokenResponse? tokenResponse;

            try
            {
                tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent);
            }
            catch (JsonException)
            {
                throw new KernelException($"Faild to deserialize token response from {openAIAuthenticationConfig.AuthorizationUrl}.");
            }

            scheme = tokenResponse?.TokenType ?? throw new KernelException("No token type found in the response.");
            credential = tokenResponse?.AccessToken ?? throw new KernelException("No access token found in the response.");
        }
        else
        {
            string token = openAIAuthenticationConfig.VerificationTokens?[pluginName]
                ?? throw new KernelException("No verification token found for the provided plugin name.");

            scheme = openAIAuthenticationConfig.AuthorizationType.ToString();
            credential = token;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(scheme, credential);
    }
}

internal sealed class OAuthTokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}

internal sealed class HttpMessageHandlerStub : DelegatingHandler
{
    public HttpResponseMessage ResponseToReturn { get; set; }

    public HttpMessageHandlerStub(string responseToReturn)
    {
        this.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseToReturn, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri!.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
        {
            return this.ResponseToReturn;
        }

        using HttpClient client = new();
        using HttpRequestMessage newRequest = new()
        {
            Content = request.Content,
            Method = request.Method,
            RequestUri = request.RequestUri,
        };

        foreach (var header in request.Headers)
        {
            newRequest.Headers.Add(header.Key, header.Value);
        }

        return await client.SendAsync(newRequest, cancellationToken).ConfigureAwait(false);
    }
}