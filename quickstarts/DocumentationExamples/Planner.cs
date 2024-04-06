namespace DocumentationExamples;

public class Planner(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Planner ========");

        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        builder.Services.AddSingleton(Output);

        builder.Plugins.AddFromType<MathSolver>();

        Kernel kernel = builder.Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory history = [];

        string userInput = "If my investment of 2130.23 dollars increased by 23%, how much would I have after I spent $5 on a latte?";

        Write("User > ");
        Write(userInput);

        history.AddUserMessage(userInput);

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(history, openAIPromptExecutionSettings, kernel);

        Write("Assistant > ");
        Write(result);

        WriteLine();

        history.AddAssistantMessage(result.ToString());
    }
}
