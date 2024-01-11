namespace KernelSyntaxExamples;

public static class Example20_HuggingFace
{
    public static async Task RunAsync()
    {
        //TODO Hugging Face 
        await RunInferenceApiExampleAsync();
        await RunLlamaExampleAsync();
    }

    private static async Task RunInferenceApiExampleAsync()
    {
        Console.WriteLine("\n======== HuggingFace Inference API example ========\n");

        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(
                model: TestConfiguration.HuggingFace.ModelId,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();

        KernelFunction questionAnswerFunction = kernel.CreateFunctionFromPrompt("Question: {{$input}}; Answer:");

        FunctionResult result = await kernel.InvokeAsync(questionAnswerFunction, new() { ["input"] = "What is New York?" });

        Console.WriteLine(result.GetValue<string>());
    }

    private static async Task RunLlamaExampleAsync()
    {
        Console.WriteLine("\n======== HuggingFace Llama 2 example ========\n");

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

        Console.WriteLine(result.GetValue<string>());
    }
}
