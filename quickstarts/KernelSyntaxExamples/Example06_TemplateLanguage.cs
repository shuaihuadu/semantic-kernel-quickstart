namespace KernelSyntaxExamples;

public static class Example06_TemplateLanguage
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== TemplateLanguage ========");

        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;
        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;

        if (string.IsNullOrEmpty(deploymentName) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Azure OpenAI credentials not found. Skipping example.");

            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
            .Build();

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        kernel.ImportPluginFromType<TimePlugin>("time");
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        const string FunctionDefinition = @"
Today is: {{time.Date}}
Current time is: {{time.Time}}

Answer to the following questions using JSON syntax, including the data used.
Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
Is it weekend time (weekend/not weekend)?
";

        Console.WriteLine("--- Rendered Prompt");

        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(FunctionDefinition));
        var renderedPrompt = await promptTemplate.RenderAsync(kernel);

        Console.WriteLine(renderedPrompt);

        KernelFunction kindOfDay = kernel.CreateFunctionFromPrompt(FunctionDefinition, new PromptExecutionSettings
        {
            ModelId = string.Empty,
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 200
            }
        });

        Console.WriteLine("--- Prompt Function result");

        FunctionResult result = await kernel.InvokeAsync(kindOfDay);

        Console.WriteLine(result.GetValue<string>());
    }
}
