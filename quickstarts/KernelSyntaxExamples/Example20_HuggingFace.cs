namespace KernelSyntaxExamples;

public class Example20_HuggingFace : BaseTest
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

    private async Task RunLlamaExampleAsync()
    {
        this.WriteLine("\n======== HuggingFace Llama 2 example ========\n");

        const string Model = "meta-llama/Llama-2-7b-hf";

        const string Endpoint = "https://api-inference.huggingface.co/models/meta-llama/Llama-2-7b-hf";

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(
                model: Model,
                endpoint: Endpoint,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        KernelFunction questionAnswerFunction = kernel.CreateFunctionFromPrompt("Question: {{$input}}; Answer:");

        FunctionResult result = await kernel.InvokeAsync(questionAnswerFunction, new() { ["input"] = "What is New York?" });

        this.WriteLine(result.GetValue<string>());
    }

    public Example20_HuggingFace(ITestOutputHelper output) : base(output)
    {
    }
}
