namespace KernelSyntaxExamples;

public static class Example64_MultiplePromptTemplates
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Example64_MultiplePromptTemplates ========");

        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (deploymentName == null
            || endpoint == null
            || apiKey == null)
        {
            Console.WriteLine("AzureOpenAI endpoint, apiKey, or deploymentName not found. Skipping example.");
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

        string skPrompt = "Hello AI, my name is {{$name}}. What is the origin of my name?";
        string handlerbarsPrompt = "Hello AI, my name if {{name}}. What is the origin of my name?";

        await RunPromptAsync(kernel, skPrompt, "semantic-kernel", promptTemplateFactory);
        await RunPromptAsync(kernel, handlerbarsPrompt, "handlebars", promptTemplateFactory);
    }

    public static async Task RunPromptAsync(Kernel kernel, string prompt, string templateFormat, IPromptTemplateFactory promptTemplateFactory)
    {
        Console.WriteLine($"======== {templateFormat} : {prompt} ========");

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

        Console.WriteLine(result.GetValue<string>());
    }
}
