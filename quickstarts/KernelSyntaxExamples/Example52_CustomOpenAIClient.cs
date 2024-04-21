namespace KernelSyntaxExamples;

public class Example52_CustomOpenAIClient(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Using a custom OpenAI client ========");

        HttpClient httpClient = new();

        httpClient.DefaultRequestHeaders.Add("My-Custom-Header", "My Custom Value");

        OpenAIClientOptions clientOptions = new()
        {
            Transport = new HttpClientTransport(httpClient)
        };

        OpenAIClient openAIClient = new(new Uri(TestConfiguration.AzureOpenAI.Endpoint), new AzureKeyCredential(TestConfiguration.AzureOpenAI.ApiKey), clientOptions);

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.DeploymentName, openAIClient);

        Kernel kernel = builder.Build();

        string folder = RepoFiles.SamplePluginsPath();

        kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "FunPlugin"));

        FunctionResult result = await kernel.InvokeAsync(
            kernel.Plugins["FunPlugin"]["Excuses"],
            new() { ["input"] = "I have no homework" });

        WriteLine(result.ToString());

        httpClient.Dispose();
    }
}