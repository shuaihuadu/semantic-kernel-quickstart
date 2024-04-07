
namespace KernelSyntaxExamples;

public class Example64_MultiplePromptTemplates : BaseTest
{
    [RetryTheory(typeof(HttpOperationException))]
    [InlineData("semantic-kernel", "Hello AI, my name is {{$name}}. What is the origin of my name?")]
    [InlineData("handlebars", "Hello AI, my name if {{name}}. What is the origin of my name?")]
    public async Task RunAsync(string templateFormat, string prompt)
    {
        this.WriteLine("======== Example64_MultiplePromptTemplates ========");

        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.DeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (deploymentName == null
            || endpoint == null
            || apiKey == null)
        {
            this.WriteLine("AzureOpenAI endpoint, apiKey, or deploymentName not found. Skipping example.");
            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey)
            .Build();

        AggregatorPromptTemplateFactory promptTemplateFactory = new(
            new KernelPromptTemplateFactory(),
            new HandlebarsPromptTemplateFactory());

        await RunPromptAsync(kernel, prompt, templateFormat, promptTemplateFactory);
    }

    private async Task RunPromptAsync(Kernel kernel, string prompt, string templateFormat, IPromptTemplateFactory promptTemplateFactory)
    {
        this.WriteLine($"======== {templateFormat} : {prompt} ========");

        KernelFunction function = kernel.CreateFunctionFromPrompt(
            promptConfig: new PromptTemplateConfig
            {
                Template = prompt,
                TemplateFormat = templateFormat,
                Name = "MyFunction"
            },
            promptTemplateFactory: promptTemplateFactory);

        KernelArguments arguments = new()
        {
            {"name","Bob" }
        };

        FunctionResult result = await kernel.InvokeAsync(function, arguments);

        this.WriteLine(result.GetValue<string>());
    }

    public Example64_MultiplePromptTemplates(ITestOutputHelper output) : base(output)
    {
    }
}
