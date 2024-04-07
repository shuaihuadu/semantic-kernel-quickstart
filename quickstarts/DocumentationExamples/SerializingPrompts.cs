using System.Reflection;

namespace DocumentationExamples;

public class SerializingPrompts : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Serializing Prompts ========");

        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Plugins.AddFromType<ConversationSummaryPlugin>();

        Kernel kernel = builder.Build();

        string pluginDirectory = Path.Join(AppContext.BaseDirectory, "Plugins", "Prompts");

        KernelPlugin prompts = kernel.CreatePluginFromPromptDirectory(pluginDirectory);

        using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.getIntent.prompt.yaml")!);

        KernelFunction getIntent = kernel.CreateFunctionFromPromptYaml(
            await reader.ReadToEndAsync(),
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        List<string> choices = ["ContinueConversation", "EndConversation"];

        List<ChatHistory> fewShotExamples =
        [
            [
                new ChatMessageContent(AuthorRole.User, "Can you send a very quick approval to the marketing team?"),
                new ChatMessageContent(AuthorRole.System, "Intent:"),
                new ChatMessageContent(AuthorRole.Assistant, "ContinueConversation")
            ],
            [
                new ChatMessageContent(AuthorRole.User, "Can you send the full update to the marketing team?"),
                new ChatMessageContent(AuthorRole.System, "Intent:"),
                new ChatMessageContent(AuthorRole.Assistant, "EndConversation")
            ]
        ];

        ChatHistory history = [];

        Write("User > ");
        string? userInput;
        while ((userInput = ReadLine()) != null)
        {
            var intent = await kernel.InvokeAsync(
                getIntent,
                new()
                {
                    { "request", userInput },
                    { "choices", choices },
                    { "history", history },
                    { "fewShotExamples", fewShotExamples }
                }
            );

            if (intent.ToString() == "EndConversation")
            {
                break;
            }

            IAsyncEnumerable<StreamingChatMessageContent> chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
                prompts["chat"],
                new()
                {
                    { "request", userInput },
                    { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
                }
            );

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

    public SerializingPrompts(ITestOutputHelper output) : base(output)
    {
        SimulatedInputText = [
            "Can you send an approval to the marketing team?",
            "That is all, thanks."];
    }
}
