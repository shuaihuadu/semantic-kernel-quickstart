namespace TextGeneration;

public class HuggingFace_TextGeneration(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunInferenceApiExampleAsync()
    {
        this.WriteLine("\n======== HuggingFace Inference API example ========\n");

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(
                //model: "microsoft/phi-2",
                model: "gpt2",
                //model: "mistralai/Mistral-7B-Instruct-v0.3",
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        KernelFunction questionAnswerFunction = kernel.CreateFunctionFromPrompt("Question: {{$input}}; Answer:");

        FunctionResult result = await kernel.InvokeAsync(questionAnswerFunction, new() { ["input"] = "What is New York?" });

        this.WriteLine(result.GetValue<string>());
    }

    [RetryFact(typeof(HttpOperationException))]
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

    [Fact]
    public async Task RunLlamaExampleAsync()
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
