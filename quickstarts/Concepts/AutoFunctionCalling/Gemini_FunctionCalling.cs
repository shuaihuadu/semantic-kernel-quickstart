namespace AutoFunctionCalling;

public class Gemini_FunctionCalling(ITestOutputHelper output) : BaseTest(output)
{
    [RetryFact(Skip = "TODO Google AI")]
    public Task GoogleAIAsync()
    {
        Console.WriteLine("============= Google AI - Gemini Chat Completion with function calling =============");

        Kernel kernel = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(
                modelId: TestConfiguration.GoogleAI.Gemini.ModelId,
                apiKey: TestConfiguration.GoogleAI.ApiKey)
            .Build();

        return Task.FromResult(0);
    }
}
