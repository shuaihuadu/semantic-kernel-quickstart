namespace KernelSyntaxExamples;

public sealed class Example83_ApiManifest : BaseTest
{
    public static readonly IEnumerable<object[]> parameters = new List<object[]>
    {
        new object[] {"MessagesPlugin","meListMessages",new KernelArguments { {"_top","1" } } ,"MessagesPlugin"}
    };

    [Theory(Skip = "ImportPluginFromApiManifestAsync"), MemberData(nameof(parameters))]
    public async Task RunSampleWithPlannerAsync(string pluginToTest, string functionToTest, KernelArguments? arguments, params string[] pluginsToLoad)
    {
        WriteSampleHeadingToConsole(pluginToTest, functionToTest, arguments, pluginsToLoad);

        Kernel kernel = Kernel.CreateBuilder().Build();

        await AddApiManifestPluginsAsync(kernel, pluginsToLoad);

        FunctionResult result = await kernel.InvokeAsync(pluginToTest, functionToTest, arguments);

        WriteLine("--------------------");
        WriteLine($"\nResult:\n{result}\n");
        WriteLine("--------------------");
    }

    private void WriteSampleHeadingToConsole(string pluginToTest, string functionToTest, KernelArguments? arguments, params string[] pluginsToLoad)
    {
        WriteLine();
        WriteLine("======== [ApiManifest Plugins Sample] ========");
        WriteLine($"======== Loading Plugins: {string.Join(" ", pluginsToLoad)} ========");
        WriteLine($"======== Calling Plugin Function: {pluginToTest}.{functionToTest} with parameters {arguments?.Select(x => x.Key + " = " + x.Value).Aggregate((x, y) => x + ", " + y)} ========");
        WriteLine();
    }

    private async Task AddApiManifestPluginsAsync(Kernel kernel, params string[] pluginNames)
    {
        //if (TestConfiguration.MsGraph.Scopes == null)
        //{
        //    throw new InvalidOperationException("Missing Scopes configuration for Microsoft Graph API.");
        //}

        //LocalUserMSALCredentialManager credentialManager = await LocalUserMSALCredentialManager.CreateAsync().ConfigureAwait(false);

        //string token = await credentialManager.GetTokenAsync(
        //    TestConfiguration.MsGraph.ClientId,
        //    TestConfiguration.MsGraph.TenantId,
        //    TestConfiguration.MsGraph.Scopes.ToArray(),
        //    TestConfiguration.MsGraph.RedirectUri).ConfigureAwait(false);

        //BearerAuthenticationProviderWithCancellationToken authenticationProvider = new(() => Task.FromResult(token));

        //foreach (var pluginName in pluginNames)
        //{
        //    try
        //    {
        //        KernelPlugin plugin = await kernel.ImportPluginFromApiManifestAsync
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
    }

    public Example83_ApiManifest(ITestOutputHelper output) : base(output)
    {
    }
}

public class BearerAuthenticationProviderWithCancellationToken
{
    private readonly Func<Task<string>> _bearerToken;

    public BearerAuthenticationProviderWithCancellationToken(Func<Task<string>> bearerToken)
    {
        this._bearerToken = bearerToken;
    }

    public async Task AuthenticateRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        string token = await this._bearerToken().ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}