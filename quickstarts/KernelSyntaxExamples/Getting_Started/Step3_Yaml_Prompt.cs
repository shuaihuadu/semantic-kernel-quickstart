namespace KernelSyntaxExamples.GettingStart;

public class Step3_Yaml_Prompt : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        string generateStoryYaml = EmbeddedResource.Read("GenerateStory.yaml");

        KernelFunction function = kernel.CreateFunctionFromPromptYaml(generateStoryYaml);

        FunctionResult result = await kernel.InvokeAsync(function, new()
        {
            {"topic","Dog" },
            { "length","3"}
        });

        WriteLine(result.ToString());

        string generateStoryHandlebarsYaml = EmbeddedResource.Read("GenerateStoryHandlebars.yaml");

        function = kernel.CreateFunctionFromPromptYaml(generateStoryHandlebarsYaml, new HandlebarsPromptTemplateFactory());

        result = await kernel.InvokeAsync(function, new()
        {
            {"topic","cat" },
            {"length","3" }
        });

        WriteLine(result.ToString());
    }

    public Step3_Yaml_Prompt(ITestOutputHelper output) : base(output)
    {
    }
}
