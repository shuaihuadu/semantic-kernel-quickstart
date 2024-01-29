namespace KernelSyntaxExamples;

public class Example26_AADAuth : BaseTest
{
    [Fact(Skip = "Setup credentials")]
    public async Task RunAsync()
    {
        this.WriteLine("======== SK with AAD Auth ========");

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
        this.WriteLine(reply);
    }

    public Example26_AADAuth(ITestOutputHelper output) : base(output)
    {
    }
}
