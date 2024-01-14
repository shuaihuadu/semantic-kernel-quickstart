namespace KernelSyntaxExamples;

public static class Example54_AzureChatCompletionWithData
{

    public static async Task RunAsync()
    {
        await ExampleWithChatCompletionAsync();
        await ExampleWithKernelAsync();
    }

    private static async Task ExampleWithChatCompletionAsync()
    {
        Console.WriteLine("=== Example with Chat Completion ===");

        AzureOpenAIChatCompletionWithDataService chatCompletionWithDataService = new(GetCompletionWithDataConfig());

        ChatHistory chatHistory = new();

        //string ask = "How did Emily and David meet?";
        string ask = "How many hotels?";
        chatHistory.AddUserMessage(ask);

        AzureOpenAIWithDataChatMessageContent chatMessage = (AzureOpenAIWithDataChatMessageContent)await chatCompletionWithDataService.GetChatMessageContentAsync(chatHistory);

        string response = chatMessage.Content!;
        string? toolResponse = chatMessage.ToolContent;

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine($"Response: {response}");
        Console.WriteLine();

        if (!string.IsNullOrEmpty(toolResponse))
        {
            chatHistory.AddMessage(AuthorRole.Tool, toolResponse);
        }

        chatHistory.AddAssistantMessage(response);

        //ask = "What are Emily and David studying?";
        ask = "How much?";

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine("Response: ");

        await foreach (var word in chatCompletionWithDataService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            Console.Write(word);
        }

        Console.WriteLine(Environment.NewLine);
    }

    private static async Task ExampleWithKernelAsync()
    {
        Console.WriteLine("=== Example with Kernel ===");

        //string ask = "How did Emily and David meet?";
        string ask = "How many hotels?";

        AzureOpenAIChatCompletionWithDataConfig completionWithDataConfig = GetCompletionWithDataConfig();

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config: completionWithDataConfig)
            .Build();

        KernelFunction function = kernel.CreateFunctionFromPrompt("Question: {{$input}}");

        FunctionResult result = await kernel.InvokeAsync(function, new() { ["input"] = ask });

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine($"Response: {result.GetValue<string>()}");
        Console.WriteLine();

        //ask = "What are Emily and David studying?";
        ask = "How much?";
        result = await kernel.InvokeAsync(function, new() { ["input"] = ask });

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine($"Response: {result.GetValue<string>()}");
        Console.WriteLine();
    }

    private static AzureOpenAIChatCompletionWithDataConfig GetCompletionWithDataConfig()
    {
        return new AzureOpenAIChatCompletionWithDataConfig
        {
            CompletionModelId = TestConfiguration.AzureOpenAI.ChatDeploymentName,
            CompletionEndpoint = TestConfiguration.AzureOpenAI.Endpoint,
            CompletionApiKey = TestConfiguration.AzureOpenAI.ApiKey,
            DataSourceEndpoint = TestConfiguration.AzureAISearch.Endpoint,
            DataSourceApiKey = TestConfiguration.AzureAISearch.ApiKey,
            DataSourceIndex = TestConfiguration.AzureAISearch.IndexName
        };
    }
}
