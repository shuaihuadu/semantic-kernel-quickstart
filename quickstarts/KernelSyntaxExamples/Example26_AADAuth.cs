namespace KernelSyntaxExamples;

public static class Example26_AADAuth
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== SK with AAD Auth ========");

        DefaultAzureCredentialOptions authOptions = new DefaultAzureCredentialOptions
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = true,
            ExcludeSharedTokenCacheCredential = true,
            ExcludeAzureCliCredential = true,
            ExcludeVisualStudioCredential = true,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeInteractiveBrowserCredential = false,
            ExcludeAzureDeveloperCliCredential = true,
            ExcludeWorkloadIdentityCredential = true,
            ExcludeAzurePowerShellCredential = true
        };

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.ChatDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                credentials: new DefaultAzureCredential(authOptions))
            .Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory chatHistory = new();

        chatHistory.AddUserMessage("Tell me a joke about hourglasses");

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        Console.WriteLine(reply);
    }
}
