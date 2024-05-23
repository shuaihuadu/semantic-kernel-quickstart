namespace LocalModels;

public class MultipleProviders_ChatCompletion(ITestOutputHelper output) : BaseTest(output)
{
    [Theory]
    [InlineData("Ollama", "http://localhost:19888", "phi3")]
    public async Task LocalModel_ExampleAsync(string messageAPIPlatform, string url, string modelId)
    {
        Console.WriteLine($"Example using local {messageAPIPlatform}");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: null,
                endpoint: new Uri(url))
            .Build();

        var prompt = @"请使用简体中文将三个```之间的文本重写为商务邮件。使用专业的语气，清晰简洁。
                   设置邮件签名为：AI Assistant.
                   文本: ```{{$input}}```";

        var mailFunction = kernel.CreateFunctionFromPrompt(prompt, new OpenAIPromptExecutionSettings
        {
            TopP = 0.5,
            MaxTokens = 1000,
        });

        var response = await kernel.InvokeAsync(mailFunction, new() { ["input"] = "告诉张三，我将在本周末前完成商业计划。" });

        Console.WriteLine(response);
    }

    [Theory]
    //[InlineData("LMStudio", "http://localhost:1234", "llama2")]
    [InlineData("Ollama", "http://localhost:19888", "phi3")]
    //[InlineData("LocalAI", "http://localhost:8080", "phi-2")]
    public async Task LocalModel_Example1Async(string messageAPIPlatform, string url, string modelId)
    {
        Console.WriteLine($"Example using local {messageAPIPlatform}");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: null,
                endpoint: new Uri(url))
            .Build();

        var prompt = @"Rewrite the text between triple backticks into a business mail. Use a professional tone, be clear and concise.
                   Sign the mail as AI Assistant.

                   Text: ```{{$input}}```";

        var mailFunction = kernel.CreateFunctionFromPrompt(prompt, new OpenAIPromptExecutionSettings
        {
            TopP = 0.5,
            MaxTokens = 1000,
        });

        var response = await kernel.InvokeAsync(mailFunction, new() { ["input"] = "Tell David that I'm going to finish the business plan by the end of the week." });
        Console.WriteLine(response);
    }

    [Theory]
    //[InlineData("LMStudio", "http://localhost:1234", "llama2")]
    [InlineData("Ollama", "http://localhost:11434", "llama2")]
    //[InlineData("LocalAI", "http://localhost:8080", "phi-2")]
    public async Task LocalModel_StreamingExampleAsync(string messageAPIPlatform, string url, string modelId)
    {
        Console.WriteLine($"Example using local {messageAPIPlatform}");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: null,
                endpoint: new Uri(url))
            .Build();

        var prompt = @"Rewrite the text between triple backticks into a business mail. Use a professional tone, be clear and concise.
                   Sign the mail as AI Assistant.

                   Text: ```{{$input}}```";

        var mailFunction = kernel.CreateFunctionFromPrompt(prompt, new OpenAIPromptExecutionSettings
        {
            TopP = 0.5,
            MaxTokens = 1000,
        });

        await foreach (var word in kernel.InvokeStreamingAsync(mailFunction, new() { ["input"] = "Tell David that I'm going to finish the business plan by the end of the week." }))
        {
            Console.WriteLine(word);
        }
    }
}
