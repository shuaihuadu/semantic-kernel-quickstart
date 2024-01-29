namespace KernelSyntaxExamples.GettingStart;

public class Step7_Observability : BaseTest
{

    [Fact]
    public async Task ObservabilityWithFiltersAsync()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        kernelBuilder.Plugins.AddFromType<TimeInformation>();

        kernelBuilder.Services.AddSingleton(this.Output);
        kernelBuilder.Services.AddSingleton<IFunctionFilter, MyFunctionFilter>();

        Kernel kernel = kernelBuilder.Build();

        kernel.PromptFilters.Add(new MyPromptFilter(this.Output));

        OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        FunctionResult result = await kernel.InvokePromptAsync("How many days until Christmas? Explain your thinking.", new KernelArguments(settings));

        WriteLine(result.ToString());
    }

    [Fact]
    [Obsolete("Events are deprecated in favor of filters.")]
    public async Task ObservabilityWithHooksAsync()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        kernelBuilder.Plugins.AddFromType<TimeInformation>();

        Kernel kernel = kernelBuilder.Build();

        void MyInvokingHandler(object? sender, FunctionInvokingEventArgs e)
        {
            WriteLine($"Invoking {e.Function.Name}");
        }

        void MyRenderingHandler(object? sender, PromptRenderingEventArgs e)
        {
            WriteLine($"Rendering prompt for {e.Function.Name}");
        }

        void MyRenderedHandler(object? sender, PromptRenderedEventArgs e)
        {
            WriteLine($"Prompt sent to model: {e.RenderedPrompt}");
        }

        void MyInvokedHandler(object? sender, FunctionInvokedEventArgs e)
        {
            if (e.Result.Metadata is not null && e.Result.Metadata.ContainsKey("Usage"))
            {
                WriteLine($"Token usage: {e.Result.Metadata?["Usage"]?.AsJson()}");
            }
        }

        kernel.FunctionInvoking += MyInvokingHandler;
        kernel.PromptRendering += MyRenderingHandler;
        kernel.PromptRendered += MyRenderedHandler;
        kernel.FunctionInvoked += MyInvokedHandler;

        OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        FunctionResult result = await kernel.InvokePromptAsync("How many days until Christmas? Explain your thinking.", new KernelArguments(settings));

        WriteLine(result.ToString());
    }

    private sealed class TimeInformation
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }

    private sealed class MyFunctionFilter : IFunctionFilter
    {
        private readonly ITestOutputHelper _output;

        public MyFunctionFilter(ITestOutputHelper output)
        {
            this._output = output;
        }

        public void OnFunctionInvoked(FunctionInvokedContext context)
        {
            IReadOnlyDictionary<string, object?>? metadata = context.Result.Metadata;

            if (metadata is not null && metadata.ContainsKey("Usage"))
            {
                this._output.WriteLine($"Token usage: {metadata["Usage"]?.AsJson()}");
            }
        }

        public void OnFunctionInvoking(FunctionInvokingContext context)
        {
            this._output.WriteLine($"Invoking {context.Function.Name}");
        }
    }

    private sealed class MyPromptFilter : IPromptFilter
    {
        private readonly ITestOutputHelper _output;

        public MyPromptFilter(ITestOutputHelper output)
        {
            this._output = output;
        }

        public void OnPromptRendered(PromptRenderedContext context)
        {
            this._output.WriteLine($"Prompt sent to model: {context.RenderedPrompt}");
        }

        public void OnPromptRendering(PromptRenderingContext context)
        {
            this._output.WriteLine($"Rendering prompt for {context.Function.Name}");
        }
    }

    public Step7_Observability(ITestOutputHelper output) : base(output)
    {
    }
}
