using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace DocumentationExamples;

public class CreatingFunction : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Creating native function ========");

        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        builder.Plugins.AddFromType<MathPlugin>();

        Kernel kernel = builder.Build();

        double answer = await kernel.InvokeAsync<double>("MathPlugin", "Sqrt", new() { ["number1"] = 12 });

        WriteLine($"The square root of 12 is {answer}.");

        ChatHistory history = [];

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        Write("User > ");

        string? userInput;

        while ((userInput = ReadLine()) != null)
        {
            history.AddUserMessage(userInput);

            OpenAIPromptExecutionSettings promptSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            IAsyncEnumerable<StreamingChatMessageContent> result = chatCompletionService.GetStreamingChatMessageContentsAsync(history, promptSettings, kernel);

            string fullMessage = "";

            bool first = true;

            await foreach (StreamingChatMessageContent content in result)
            {
                if (content.Role.HasValue && first)
                {
                    Write("Assistant > ");
                    first = false;
                }

                Write(content.Content);

                fullMessage += content.Content;
            }

            WriteLine();

            history.AddAssistantMessage(fullMessage);

            Write("User > ");
        }
    }
    public CreatingFunction(ITestOutputHelper output) : base(output)
    {
        SimulatedInputText = ["What is 49 divided by 37?"];
    }
}