namespace KernelSyntaxExamples.GettingStart;

public class Step2_Add_Plugins : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        kernelBuilder.Plugins.AddFromType<TimeInformation>();

        Kernel kernel = kernelBuilder.Build();

        WriteLine(await kernel.InvokePromptAsync("How many days until Christmas?"));

        WriteLine(await kernel.InvokePromptAsync("The current time is {{TimeInformation.GetCurrentUtcTime}}. How many days until Christmas?"));

        OpenAIPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        WriteLine(await kernel.InvokePromptAsync("How many days until Christmas? Explain your thinking.", new KernelArguments(settings)));
    }

    public class TimeInformation
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }

    public Step2_Add_Plugins(ITestOutputHelper output) : base(output)
    {
    }
}