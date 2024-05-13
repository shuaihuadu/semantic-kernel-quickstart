namespace ChatCompletion;

public class AzureOpenAIWithData_ChatCompletion(ITestOutputHelper output) : BaseTest(output)
{
    // https://learn.microsoft.com/en-us/azure/ai-services/openai/use-your-data-quickstart

    [RetryFact(typeof(HttpOperationException))]
    public async Task ExampleWithChatCompletionAsync()
    {
        this.WriteLine("=== Example with Chat Completion ===");

        AzureOpenAIChatCompletionWithDataService chatCompletionWithDataService = new(GetCompletionWithDataConfig());

        ChatHistory chatHistory = [];

        //string ask = "How did Emily and David meet?";
        string ask = "How many hotels?";
        chatHistory.AddUserMessage(ask);

        AzureOpenAIWithDataChatMessageContent chatMessage = (AzureOpenAIWithDataChatMessageContent)await chatCompletionWithDataService.GetChatMessageContentAsync(chatHistory);

        string response = chatMessage.Content!;
        string? toolResponse = chatMessage.ToolContent;

        this.WriteLine($"Ask: {ask}");
        this.WriteLine($"Response: {response}");
        this.WriteLine();

        if (!string.IsNullOrEmpty(toolResponse))
        {
            chatHistory.AddMessage(AuthorRole.Tool, toolResponse);
        }

        chatHistory.AddAssistantMessage(response);

        //ask = "What are Emily and David studying?";
        ask = "Where is the Triple Landscape Hotel?";

        this.WriteLine($"Ask: {ask}");
        this.WriteLine("Response: ");

        await foreach (var word in chatCompletionWithDataService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            this.Write(word);
        }

        this.WriteLine(Environment.NewLine);
    }

    [RetryFact(typeof(HttpOperationException))]
    private async Task ExampleWithKernelAsync()
    {
        this.WriteLine("=== Example with Kernel ===");

        //string ask = "How did Emily and David meet?";
        string ask = "How many hotels?";

        AzureOpenAIChatCompletionWithDataConfig completionWithDataConfig = GetCompletionWithDataConfig();

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config: completionWithDataConfig)
            .Build();

        KernelFunction function = kernel.CreateFunctionFromPrompt("Question: {{$input}}");

        FunctionResult result = await kernel.InvokeAsync(function, new() { ["input"] = ask });

        this.WriteLine($"Ask: {ask}");
        this.WriteLine($"Response: {result.GetValue<string>()}");
        this.WriteLine();

        //ask = "What are Emily and David studying?";
        ask = "Where is the Triple Landscape Hotel?";
        result = await kernel.InvokeAsync(function, new() { ["input"] = ask });

        this.WriteLine($"Ask: {ask}");
        this.WriteLine($"Response: {result.GetValue<string>()}");
        this.WriteLine();
    }

    private static AzureOpenAIChatCompletionWithDataConfig GetCompletionWithDataConfig()
    {
        //TestConfiguration.Initialize();

        return new AzureOpenAIChatCompletionWithDataConfig
        {
            CompletionModelId = TestConfiguration.AzureOpenAI.DeploymentName,
            CompletionEndpoint = TestConfiguration.AzureOpenAI.Endpoint,
            CompletionApiKey = TestConfiguration.AzureOpenAI.ApiKey,
            DataSourceEndpoint = TestConfiguration.AzureAISearch.Endpoint,
            DataSourceApiKey = TestConfiguration.AzureAISearch.ApiKey,
            DataSourceIndex = TestConfiguration.AzureAISearch.IndexName
        };
    }
}
