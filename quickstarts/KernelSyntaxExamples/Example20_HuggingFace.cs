namespace KernelSyntaxExamples;

public class Example20_HuggingFace(ITestOutputHelper output) : BaseTest(output)
{
    [Fact(Skip = "TODO Hugging Face ")]
    public async Task RunAsync()
    {
        //TODO Hugging Face 
        await RunInferenceApiExampleAsync();
        await RunLlamaExampleAsync();
    }

    private async Task RunInferenceApiExampleAsync()
    {
        this.WriteLine("\n======== HuggingFace Inference API example ========\n");

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(
                model: TestConfiguration.HuggingFace.ModelId,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        KernelFunction questionAnswerFunction = kernel.CreateFunctionFromPrompt("Question: {{$input}}; Answer:");

        FunctionResult result = await kernel.InvokeAsync(questionAnswerFunction, new() { ["input"] = "What is New York?" });

        this.WriteLine(result.GetValue<string>());
    }

    [RetryFact(typeof(HttpOperationException), Skip = "TODO Hugging Face ")]
    public async Task RunInferenceApiEmbeddingAsync()
    {
        this.WriteLine("\n======= Hugging Face Inference API - Embedding Example ========\n");

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextEmbeddingGeneration(
                model: TestConfiguration.HuggingFace.EmbeddingModelId,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        ITextEmbeddingGenerationService embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        IList<ReadOnlyMemory<float>> embeddings = await embeddingGenerator.GenerateEmbeddingsAsync(["John: Hello, how are you?\nRoger: Hey, I'm Roger!"]);

        WriteLine($"Generated {embeddings.Count} embeddings for the provided text");
    }

    [RetryFact(typeof(HttpOperationException), Skip = "TODO Hugging Face ")]
    public async Task RunStreamingExampleAsync()
    {
        WriteLine("\n======== HuggingFace zephyr-7b-beta streaming example ========\n");

        const string Model = "HuggingFaceH4/zephyr-7b-beta";

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(
                model: Model,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        HuggingFacePromptExecutionSettings settings = new HuggingFacePromptExecutionSettings
        {
            UseCache = false
        };

        KernelFunction questionAnswerFunction = kernel.CreateFunctionFromPrompt("Question: {{$input}}; Answer:", new HuggingFacePromptExecutionSettings
        {
            UseCache = false
        });

        await foreach (string text in kernel.InvokePromptStreamingAsync<string>("Qustion: {{$input}}; Answer:", new KernelArguments(settings) { ["input"] = "What is New York?" }))
        {
            Write(text);
        };
    }

    [Fact(Skip = "Requires local model or Huggingface Pro subscription")]
    private async Task RunLlamaExampleAsync()
    {
        this.WriteLine("\n======== HuggingFace Llama 2 example ========\n");

        const string Model = "meta-llama/Llama-2-7b-hf";


        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(
                model: Model,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        KernelFunction questionAnswerFunction = kernel.CreateFunctionFromPrompt("Question: {{$input}}; Answer:");

        FunctionResult result = await kernel.InvokeAsync(questionAnswerFunction, new() { ["input"] = "What is New York?" });

        this.WriteLine(result.GetValue<string>());
    }
}
