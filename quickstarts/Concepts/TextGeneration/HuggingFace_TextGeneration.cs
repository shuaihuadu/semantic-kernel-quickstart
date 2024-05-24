namespace TextGeneration;

public class HuggingFace_TextGeneration(ITestOutputHelper output) : BaseTest(output)
{
    [Fact(Skip = "TODO Hugging Face ")]
    public async Task RunInferenceApiExampleAsync()
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
    public async Task RunStreamingExampleAsync()
    {
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
            Console.Write(text);
        };
    }

    [Fact(Skip = "Requires local model or Huggingface Pro subscription")]
    private async Task RunLlamaExampleAsync()
    {
        Console.WriteLine("\n======== HuggingFace Llama 2 example ========\n");

        const string Model = "meta-llama/Llama-2-7b-hf";


        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(
                model: Model,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        KernelFunction questionAnswerFunction = kernel.CreateFunctionFromPrompt("Question: {{$input}}; Answer:");

        FunctionResult result = await kernel.InvokeAsync(questionAnswerFunction, new() { ["input"] = "What is New York?" });

        Console.WriteLine(result.GetValue<string>());
    }
}
