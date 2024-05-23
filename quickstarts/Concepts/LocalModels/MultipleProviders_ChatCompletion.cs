namespace LocalModels;

public class MultipleProviders_ChatCompletion(ITestOutputHelper output) : BaseTest(output)
{
    // # Ollama https://ollama.com
    // 1.docker pull ollama/ollama
    // 2.CPU only: docker run -d -v ollama:/root/.ollama -p 11434:11434 --name ollama ollama/ollama
    // 3.Run a model : docker exec -it ollama ollama run llama2
    //                 docker exec -it ollama ollama run llama3
    //                 docker exec -it ollama ollama run phi3
    //                 docker exec -it ollama ollama run mistral
    // https://ollama.com/library

    [Theory]
    //[InlineData("LMStudio", "http://localhost:1234", "llama2")]
    [InlineData("Ollama", "http://localhost:11434", "llama2")]
    //[InlineData("LocalAI", "http://localhost:8080", "phi-2")]
    public async Task LocalModel_ExampleAsync(string messageAPIPlatform, string url, string modelId)
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
