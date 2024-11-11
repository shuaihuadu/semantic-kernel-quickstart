using Azure.AI.OpenAI.Chat;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace ChatCompletion;

public class AzureOpenAIWithData_ChatCompletion(ITestOutputHelper output) : BaseTest(output)
{
    // https://learn.microsoft.com/en-us/azure/ai-services/openai/use-your-data-quickstart

    [RetryFact(typeof(HttpOperationException))]
    public async Task ExampleWithChatCompletionAsync()
    {
        Console.WriteLine("=== Example with Chat Completion ===");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
            TestConfiguration.AzureOpenAI.DeploymentName,
            TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        ChatHistory chatHistory = [];

        string ask = "How did Emily and David meet?";

        chatHistory.AddUserMessage(ask);

        AzureSearchChatDataSource dataSource = GetAzureSearchChatDataSource();

        AzureOpenAIPromptExecutionSettings promptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            AzureChatDataSource = dataSource
        };

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent chatMessage = await chatCompletion.GetChatMessageContentAsync(chatHistory, promptExecutionSettings);

        string response = chatMessage.Content!;

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine($"Response: {response}");

        IReadOnlyList<ChatCitation> citations = GetChatCitations(chatMessage);

        OutputCitations(citations);

        Console.WriteLine();

        chatHistory.AddAssistantMessage(response);

        ask = "What are Emily and David studying?";
        chatHistory.AddUserMessage(ask);

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine("Response: ");

        await foreach (var word in chatCompletion.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings))
        {
            Console.Write(word);
        }

        citations = GetChatCitations(chatMessage);

        OutputCitations(citations);

        Console.WriteLine(Environment.NewLine);
    }

    [RetryFact(typeof(HttpOperationException))]
    private async Task ExampleWithKernelAsync()
    {
        Console.WriteLine("=== Example with Kernel ===");

        string ask = "How did Emily and David meet?";

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.DeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        KernelFunction function = kernel.CreateFunctionFromPrompt("Question: {{$input}}");

        FunctionResult result = await kernel.InvokeAsync(function, new() { ["input"] = ask });

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine($"Response: {result.GetValue<string>()}");
        Console.WriteLine();

        //ask = "What are Emily and David studying?";
        ask = "Where is the Triple Landscape Hotel?";
        result = await kernel.InvokeAsync(function, new() { ["input"] = ask });

        Console.WriteLine($"Ask: {ask}");
        Console.WriteLine($"Response: {result.GetValue<string>()}");
        Console.WriteLine();
    }

    private static AzureSearchChatDataSource GetAzureSearchChatDataSource()
    {
        return new AzureSearchChatDataSource
        {
            Endpoint = new Uri(TestConfiguration.AzureAISearch.Endpoint),
            Authentication = DataSourceAuthentication.FromApiKey(TestConfiguration.AzureAISearch.ApiKey),
            IndexName = TestConfiguration.AzureAISearch.IndexName
        };
    }

    private static IReadOnlyList<ChatCitation> GetChatCitations(ChatMessageContent chatMessageContent)
    {
        OpenAI.Chat.ChatCompletion? message = chatMessageContent.InnerContent as OpenAI.Chat.ChatCompletion;

        ChatMessageContext messageContext = message.GetMessageContext();

        return messageContext.Citations;
    }

    private void OutputCitations(IReadOnlyList<ChatCitation> citations)
    {
        Console.WriteLine("Citations:");

        foreach (var citation in citations)
        {
            Console.WriteLine($"Chunk ID: {citation.ChunkId}");
            Console.WriteLine($"Title: {citation.Title}");
            Console.WriteLine($"File Path:{citation.FilePath}");
            Console.WriteLine($"URI:{citation.Uri}");
            Console.WriteLine($"Content: {citation.Content}");
        }
    }
}
