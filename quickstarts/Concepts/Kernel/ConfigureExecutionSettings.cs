namespace KernelSyntaxExamples;

public class ConfigureExecutionSettings(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Example58_ConfigureExecutionSettings ========");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        string prompt = "Hello AI, What can you do for me?";

        FunctionResult result = await kernel.InvokePromptAsync(
            prompt,
            new(new OpenAIPromptExecutionSettings
            {
                MaxTokens = 60,
                Temperature = 0.7
            }));

        this.WriteLine(result.GetValue<string>());

        string configPayload = @"{
            ""schema"":1,
            ""name"":""HelloAI"",
            ""description"":""Say hello to an AI"",
            ""type"":""completion"",
            ""completion"":{
                ""max_tokens"":256,
                ""temperature"":0.5,
                ""top_p"":0.0,
                ""presence_penalty"":0.0,
                ""frequency_penalty"":0.0
            }
        }";

        PromptTemplateConfig promptTemplateConfig = JsonSerializer.Deserialize<PromptTemplateConfig>(configPayload)!;

        promptTemplateConfig.Template = prompt;

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt(promptTemplateConfig);

        result = await kernel.InvokeAsync(kernelFunction);

        this.WriteLine(result.GetValue<string>());
    }
}