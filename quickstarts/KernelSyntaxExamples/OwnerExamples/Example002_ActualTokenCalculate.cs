namespace KernelSyntaxExamples.OwnerExamples;

public class Example002_ActualTokenCalculate(ITestOutputHelper output) : BaseTest(output)
{

    [Fact]
    public async Task RunAsync()
    {
        string content = "How are you";

        List<Dictionary<string, string>> exampleMessages =
        [
            new() {
                {"role", "system"},
                {"content", "Assistant is a large language model."}
            },
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

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        FunctionResult functionResult = await kernel.InvokePromptAsync(content);

        //exampleMessages =
        //[
        //    new() {
        //        //{"role", "assistant"},
        //        {"content", functionResult.ToString()}
        //    }
        //];

        WriteLine(functionResult.Metadata?["Usage"]?.AsJson());

        //count = CalculateTokensCountForMessage(exampleMessages);

        //WriteLine(count);


        //ChatHistory history = [];

        //history.AddUserMessage(content);

        //IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        //ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(history);

        //exampleMessages =
        //[
        //    new() {
        //        //{"role", "assistant"},
        //        {"content", reply.ToString()}
        //    }
        //];

        //WriteLine(reply.Metadata["Usage"].AsJson());

        //count = CalculateTokensCountForMessage(exampleMessages);

        //WriteLine(count);
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