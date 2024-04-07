namespace KernelSyntaxExamples;

public class Example06_TemplateLanguage(ITestOutputHelper output) : BaseTest(output)
{

    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== TemplateLanguage ========");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        kernel.ImportPluginFromType<TimePlugin>("time");

        const string FunctionDefinition = @"
Today is: {{time.Date}}
Current time is: {{time.Time}}

Answer to the following questions using JSON syntax, including the data used.
Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
Is it weekend time (weekend/not weekend)?
";

        this.WriteLine("--- Rendered Prompt");

        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(FunctionDefinition));
        var renderedPrompt = await promptTemplate.RenderAsync(kernel);

        this.WriteLine(renderedPrompt);

        KernelFunction kindOfDay = kernel.CreateFunctionFromPrompt(FunctionDefinition, new PromptExecutionSettings
        {
            ModelId = string.Empty,
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 200
            }
        });

        this.WriteLine("--- Prompt Function result");

        FunctionResult result = await kernel.InvokeAsync(kindOfDay);

        this.WriteLine(result.GetValue<string>());
    }
}
