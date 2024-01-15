namespace KernelSyntaxExamples;

public static class Example58_ConfigureExecutionSettings
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Example58_ConfigureExecutionSettings ========");


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

        string prompt = "Hello AI, What can you do for me?";

        FunctionResult result = await kernel.InvokePromptAsync(
            prompt,
            new(new OpenAIPromptExecutionSettings
            {
                MaxTokens = 60,
                Temperature = 0.7
            }));

        Console.WriteLine(result.GetValue<string>());

        string configPayload = @"{
            ""schema"":1,
            ""name"":""HelloAI"",
            ""description"":""Say hello to an AI"",
            ""type"":""completion"",
            ""completion"":{
                ""max_tokens"":256,
                ""temperature"":0.5,
                ""top_p"":0.0,
                ""presence_penalty"":0.0,
                ""frequency_penalty"":0.0
            }
        }";

        PromptTemplateConfig promptTemplateConfig = JsonSerializer.Deserialize<PromptTemplateConfig>(configPayload)!;

        promptTemplateConfig.Template = prompt;

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt(promptTemplateConfig);

        result = await kernel.InvokeAsync(kernelFunction);

        Console.WriteLine(result.GetValue<string>());
    }
}
