
namespace DocumentationExamples;

public class FunctionsWithPrompts : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        builder.Plugins.AddFromType<ConversationSummaryPlugin>();

        builder.Services.AddSingleton(this.Output);
        builder.Services.AddSingleton<IPromptFilter, PromptFilter>();


        Kernel kernel = builder.Build();

        List<string> choices = ["ContinueConversation", "EndConversation"];

        List<ChatHistory> fewShotExamples =
        [
            [
                new ChatMessageContent(AuthorRole.User,"Can you send a very quick approval to the marketing team?"),
                new ChatMessageContent(AuthorRole.System,"Intent:"),
                new ChatMessageContent(AuthorRole.Assistant,"ContinueConversation"),
            ],
            [
                new ChatMessageContent(AuthorRole.User, "Can you send the full update to the marketing team?"),
                new ChatMessageContent(AuthorRole.System, "Intent:"),
                new ChatMessageContent(AuthorRole.Assistant, "EndConversation")
            ]
        ];


        KernelFunction getIntent = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig
            {
                Template = @"
<message role=""system"">Instructions: What is the intent of this request?
Do not explain the reasoning, just reply back with the intent. If you are unsure, reply with {{choices[0]}}.
Choices: {{choices}}.</message>

{{#each fewShotExamples}}
    {{#each this}}
        <message role=""{{role}}"">{{content}}</message>
    {{/each}}
{{/each}}

{{ConversationSummaryPlugin-SummarizeConversation history}}

<message role=""user"">{{request}}</message>
<message role=""system"">Intent:</message>",
                TemplateFormat = "handlebars"
            }, new HandlebarsPromptTemplateFactory());

        KernelFunction chat = kernel.CreateFunctionFromPrompt(@"
{{ConversationSummaryPlugin.SummarizeConversation $history}}
User: {{$request}}
Assistant: ");

        ChatHistory history = [];

        while (true)
        {
            Write("User > ");

            string? request = ReadLine();

            WriteLine(request);

            FunctionResult intent = await kernel.InvokeAsync(getIntent, new()
            {
                ["request"] = request,
                ["choices"] = choices,
                ["history"] = history,
                //["history"] = string.Join("\n", history.Select(x => x.Role + ":" + x.Content)),
                ["fewShotExamples"] = fewShotExamples
            });

            WriteLine($"Intent: {intent}");

            if (intent.ToString() == "EndConversation")
            {
                break;
            }

            IAsyncEnumerable<StreamingChatMessageContent> chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
                chat, new()
                {
                    ["request"] = request,
                    ["history"] = string.Join("\n", history.Select(x => x.Role + ":" + x.Content))
                });

            string message = "";

            await foreach (StreamingChatMessageContent content in chatResult)
            {
                if (content.Role.HasValue)
                {
                    Write(content.Role + " > ");
                }

                message += content;

                Write(content);
            }

            WriteLine();

            history.AddUserMessage(request!);
            history.AddAssistantMessage(message);
        }

    }

    public FunctionsWithPrompts(ITestOutputHelper output) : base(output)
    {
        SimulatedInputText =
        [
            "Can you send an approval to the marketing team?",
            "That is all, thanks."
        ];
    }

    private sealed class PromptFilter(ITestOutputHelper output) : IPromptFilter
    {
        private readonly ITestOutputHelper _output = output;

        public void OnPromptRendered(PromptRenderedContext context)
        {
            this._output.WriteLine("======== RenderedPrompt Start ========");
            this._output.WriteLine(context.RenderedPrompt);
            this._output.WriteLine("======== RenderedPrompt End ========");
        }

        public void OnPromptRendering(PromptRenderingContext context)
        {

        }
    }
}
