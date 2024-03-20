namespace DocumentationExamples;

public class Templates(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Templates ========");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        KernelFunction chatFunction = kernel.CreateFunctionFromPrompt(
            @"{{$history}}
            User: {{$request}}
            Assistant: ");

        List<string> choices = ["ContinueConversation", "EndConversation"];

        List<ChatHistory> fewShotExamples =
        [
            [
                new ChatMessageContent(AuthorRole.User,"Can you send a very quick approval to the marketing team?"),
                new ChatMessageContent(AuthorRole.System,"Intent:"),
                new ChatMessageContent(AuthorRole.Assistant,"ContinueConversation")
            ],
            [
                new ChatMessageContent(AuthorRole.User,"Thanks, I'm done for now"),
                new ChatMessageContent(AuthorRole.System,"Intent:"),
                new ChatMessageContent(AuthorRole.Assistant,"EndConversation")
            ]
        ];

        KernelFunction getIntent = kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig()
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

{{#each chatHistory}}
    <message role=""{{role}}"">{{content}}</message>
{{/each}}

<message role=""user"">{{request}}</message>
<message role=""system"">Intent:</message>",
                TemplateFormat = "handlebars"
            }, new HandlebarsPromptTemplateFactory());

        ChatHistory chatHistory = [];

        while (true)
        {
            Write("User > ");

            string? request = ReadLine();

            FunctionResult intent = await kernel.InvokeAsync(getIntent, new()
            {
                {"request",request },
                {"choices",choices},
                {"history",chatHistory},
                {"fewShotExamples",fewShotExamples}
            });

            if (intent.ToString() == "EndConversation")
            {
                break;
            }

            IAsyncEnumerable<StreamingChatMessageContent> chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(chatFunction, new()
            {
                {"request",request },
                { "history",string.Join("\n",chatHistory.Select(x=>x.Role + ":" + x.Content))}
            });

            string message = "";

            await foreach (StreamingChatMessageContent chunk in chatResult)
            {
                if (chunk.Role.HasValue)
                {
                    Write(chunk.Role + " > ");
                }

                message += chunk;
                Write(chunk);
            }

            WriteLine();

            chatHistory.AddUserMessage(request!);
            chatHistory.AddAssistantMessage(message);
        }
    }
}