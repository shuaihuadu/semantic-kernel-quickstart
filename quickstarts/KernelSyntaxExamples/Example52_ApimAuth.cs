namespace KernelSyntaxExamples;

public static class Example52_ApimAuth
{
    public static async Task RunAsync()
    {
        Uri apimUri = new Uri(Env.Var("Apim__Endpoint"));
        string subscriptionKey = Env.Var("Apim__SubscriptionKey");

        string[] scopes = ["https://cognitiveservices.azure.com/.default"];

        InteractiveBrowserCredential credential = new();

        TokenRequestContext requestContext = new(scopes);

        AccessToken accessToken = await credential.GetTokenAsync(requestContext);

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

        OpenAIClientOptions clientOptions = new()
        {
            Transport = new HttpClientTransport(httpClient),
            Diagnostics =
            {
                LoggedHeaderNames = {"ErrorSource","ErrorReason","ErrorMessage","ErrorScope","ErrorSection","ErrorStatusCode" }
            }
        };

        OpenAIClient openAIClient = new(apimUri, new BearerTokenCredential(accessToken), clientOptions);

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Warning).AddConsole());
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
            openAIClient: openAIClient);

        Kernel kernel = builder.Build();

        string folder = RepoFiles.SamplePluginsPath();

        kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "FunPlugin"));

        FunctionResult result = await kernel.InvokeAsync(
            kernel.Plugins["FunPlugin"]["Excuses"],
            new() { ["input"] = "I have no homework" });

        Console.WriteLine(result.GetValue<string>());

        httpClient.Dispose();
    }
}

public class BearerTokenCredential : TokenCredential
{
    private readonly AccessToken _accessToken;

    public BearerTokenCredential(AccessToken accessToken)
    {
        this._accessToken = accessToken;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return this._accessToken;
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(this._accessToken);
    }
}
