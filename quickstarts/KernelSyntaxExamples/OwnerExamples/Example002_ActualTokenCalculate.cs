namespace KernelSyntaxExamples.OwnerExamples;

public class Example002_ActualTokenCalculate(ITestOutputHelper output) : BaseTest(output)
{

    [Fact]
    public async Task RunAsync()
    {
        string content = "How are you";

        List<Dictionary<string, string>> exampleMessages =
        [
            //new() {
            //    {"role", "system"},
            //    {"content", "You are a helpful assistant."}
            //},
            new() {
                {"role", "user"},
                {"content", content}
            },
            //new() {
            //    {"role", "assistant"},
            //    {"content", "Hello there, how may I assist you today?"}
            //}
        ];

        int count = CalculateTokensCountForMessage(exampleMessages);

        WriteLine(count);

        Kernel kernel = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        kernel.PromptFilters.Add(new PromptFilter());
        kernel.FunctionFilters.Add(new FunctionFilter());

        FunctionResult functionResult = await kernel.InvokePromptAsync(content);

        WriteLine(functionResult.Metadata["Usage"].AsJson());


        ChatHistory history = [];

        history.AddUserMessage(content);

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(history);

        WriteLine(reply.Metadata["Usage"].AsJson());
    }


    public int CalculateTokensCountForMessage(List<Dictionary<string, string>> messages)
    {
        int tokensPerMessage = 3;

        GptEncoding encoding = GptEncoding.GetEncoding("cl100k_base");
        //GptEncoding encoding = GptEncoding.GetEncodingForModel("gpt-4");

        int numTokens = 0;

        foreach (var message in messages)
        {
            numTokens += tokensPerMessage;

            foreach (var item in message)
            {
                List<int> tokens = encoding.Encode(item.Value);

                numTokens += tokens.Count;
            }
        }

        numTokens += 3; //every reply is primed with <|start|>assistant<|message|>

        return numTokens;
    }
}

public sealed class PromptFilter : IPromptFilter
{
    public void OnPromptRendered(PromptRenderedContext context)
    {
    }

    public void OnPromptRendering(PromptRenderingContext context)
    {
    }
}

public sealed class FunctionFilter : IFunctionFilter
{
    public void OnFunctionInvoked(FunctionInvokedContext context)
    {
    }

    public void OnFunctionInvoking(FunctionInvokingContext context)
    {
    }
}