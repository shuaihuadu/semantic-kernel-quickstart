namespace DocumentationExamples;

public class ConfiguringPrompts : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Configuring Prompts ========");

        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Plugins.AddFromType<ConversationSummaryPlugin>();

        Kernel kernel = builder.Build();

        KernelFunction chat = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig
            {
                Name = "Chat",
                Description = "Chat with the assistant.",
                Template =
@"{{ConversationSummaryPlugin.SummarizeConversation $history}}
User: {{$request}}
Assistant: ",
                TemplateFormat = "semantic-kernel",
                InputVariables =
                [
                    new(){ Name="history",Description="The history of the conversation.",IsRequired = false,Default = "" },
                    new(){ Name="request",Description="The user's request.",IsRequired = true }
                ],
                ExecutionSettings =
                {
                    {
                        "default",
                        new OpenAIPromptExecutionSettings()
                        {
                            MaxTokens = 1000,
                            Temperature = 0
                        }
                    },
                    {
                        "gpt-3.5-turbo",
                        new OpenAIPromptExecutionSettings()
                        {
                            ModelId = "gpt-3.5-turbo-0613",
                            MaxTokens = 1000,
                            Temperature = 0.2
                        }
                    },
                    {
                        "gpt-4",
                        new OpenAIPromptExecutionSettings()
                        {
                            ModelId = "gpt-4-1106-preview",
                            MaxTokens = 1000,
                            Temperature = 0.3
                        }
                    }
                }
            });

        ChatHistory history = [];

        Write("User > ");

        string? userInput;

        while ((userInput = ReadLine()) != null)
        {
            Write(userInput);

            IAsyncEnumerable<StreamingChatMessageContent> chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
                chat,
                new()
                {
                    ["request"] = userInput,
                    ["history"] = string.Join("\n", history.Select(x => x.Role + ": " + x.Content))
                });

            string message = "";

            await foreach (var chunk in chatResult)
            {
                if (chunk.Role.HasValue)
                {
                    Write(chunk.Role + " > ");
                }
                message += chunk;
                Write(chunk);
            }

            WriteLine();

            history.AddUserMessage(userInput);
            history.AddAssistantMessage(message);

            Write("User > ");
        }
    }

    public ConfiguringPrompts(ITestOutputHelper output) : base(output)
    {
        SimulatedInputText = ["Who were the Vikings?"];
    }
}
