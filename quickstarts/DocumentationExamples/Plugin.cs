namespace DocumentationExamples;

public class Plugin : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Plugin ========");

        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        builder.Plugins.AddFromType<LightPlugin>();

        builder.Services.AddSingleton(this.Output);

        Kernel kernel = builder.Build();

        ChatHistory history = [];

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        Write("User > ");

        string? userInput;

        while ((userInput = ReadLine()) != null)
        {
            Write(userInput);

            history.AddUserMessage(userInput);

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            ChatMessageContent? result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            WriteLine("Assistant > " + result);

            history.AddMessage(result.Role, result.Content ?? string.Empty);

            Write("User > ");
        }
    }

    public Plugin(ITestOutputHelper output) : base(output)
    {
        SimulatedInputText = [
            "Hello",
            "Can you turn on the lights"];
    }
}

public class LightPlugin
{
    private readonly ITestOutputHelper _output;

    public LightPlugin(ITestOutputHelper output)
    {
        this._output = output;
    }

    public bool IsOn { get; set; } = false;

    [KernelFunction]
    [Description("Gets the state of the light.")]
    public string GetState() => IsOn ? "on" : "off";

    [KernelFunction]
    [Description("Changes the state of the light.")]
    public string ChangeState(bool newState)
    {
        this.IsOn = newState;

        string state = this.GetState();

        this._output.WriteLine($"[Light is now {state}]");

        return state;
    }
}