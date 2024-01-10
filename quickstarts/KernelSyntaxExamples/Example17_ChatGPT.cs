﻿namespace KernelSyntaxExamples;

public static class Example17_ChatGPT
{
    public static async Task RunAsync()
    {
        await AzureOpenAIChatSampleAsync();
    }

    private static async Task AzureOpenAIChatSampleAsync()
    {
        Console.WriteLine("======== Azure Open AI - ChatGPT ========");

        AzureOpenAIChatCompletionService chatCompletionService = new(
            TestConfiguration.AzureOpenAI.ChatDeploymentName, TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey);

        await StartChatAsync(chatCompletionService);
    }

    private static async Task StartChatAsync(IChatCompletionService chatCompletionService)
    {
        Console.WriteLine("Chat content:");
        Console.WriteLine("------------------------");

        var chatHistory = new ChatHistory("You are a librarian, expert about books");

        chatHistory.AddUserMessage("Hi, I'm looking for book suggestions");
        await MessageOutputAsync(chatHistory);

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);

        chatHistory.AddUserMessage("I love history and philosophy, I'd like to learn something new about Greece, any suggestion");
        await MessageOutputAsync(chatHistory);

        reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);
        await MessageOutputAsync(chatHistory);
    }

    private static Task MessageOutputAsync(ChatHistory chatHistory)
    {
        ChatMessageContent message = chatHistory.Last();

        Console.WriteLine($"{message.Role}: {message.Content}");
        Console.WriteLine("------------------------");

        return Task.CompletedTask;
    }
}
