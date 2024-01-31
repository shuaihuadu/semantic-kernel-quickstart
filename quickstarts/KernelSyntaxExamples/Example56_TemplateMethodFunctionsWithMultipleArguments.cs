
namespace KernelSyntaxExamples;

public class Example56_TemplateMethodFunctionsWithMultipleArguments : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== TemplateMethodFunctionsWithMultipleArguments ========");

        string serviceId = TestConfiguration.AzureOpenAI.ServiceId;
        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.DeploymentName;
        string modelId = TestConfiguration.AzureOpenAI.ChatModelId;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (apiKey == null || deploymentName == null || modelId == null || endpoint == null)
        {
            this.WriteLine("AzureOpenAI modelId, endpoint, apiKey, or deploymentName not found. Skipping example.");
            return;
        }

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(configure => configure.AddConsole());

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            serviceId: serviceId,
            apiKey: apiKey,
            modelId: modelId);

        Kernel kernel = builder.Build();

        KernelArguments arguments = new();

        arguments["word2"] = " Potter";

        kernel.ImportPluginFromType<TextPlugin>("text");

        const string FunctionDefinition = @"Write a haiku about the following: {{text.Concat input='Harry' input2=$word2}}";

        this.WriteLine("--- Rendered Prompt");

        KernelPromptTemplateFactory promptTemplateFactory = new();

        IPromptTemplate promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(FunctionDefinition));
        string renderedPrompt = await promptTemplate.RenderAsync(kernel, arguments);

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt(FunctionDefinition, new OpenAIPromptExecutionSettings
        {
            MaxTokens = 100
        });

        this.WriteLine("--- Prompt Function result");

        FunctionResult result = await kernel.InvokeAsync(kernelFunction, arguments);

        this.WriteLine(result.GetValue<string>());
    }

    public Example56_TemplateMethodFunctionsWithMultipleArguments(ITestOutputHelper output) : base(output)
    {
    }
}
