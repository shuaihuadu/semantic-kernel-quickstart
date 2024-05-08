namespace KernelSyntaxExamples.GettingStart;

public class Step7_Observability(ITestOutputHelper output) : BaseTest(output)
{

    [Fact]
    public async Task ObservabilityWithFiltersAsync()
    {
        IKernelBuilder kernelBuilder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        kernelBuilder.Plugins.AddFromType<TimeInformation>();

        kernelBuilder.Services.AddSingleton(this.Output);
        kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, MyFunctionFilter>();

        Kernel kernel = kernelBuilder.Build();

        kernel.PromptRenderFilters.Add(new MyPromptFilter(this.Output));

        OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        FunctionResult result = await kernel.InvokePromptAsync("How many days until Christmas? Explain your thinking.", new KernelArguments(settings));

        WriteLine(result.ToString());
    }

    [Fact]
    [Obsolete("Events are deprecated in favor of filters.")]
    public async Task ObservabilityWithHooksAsync()
    {
        IKernelBuilder kernelBuilder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

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

    private sealed class MyFunctionFilter(ITestOutputHelper output) : IFunctionInvocationFilter
    {
        private readonly ITestOutputHelper _output = output;

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            this._output.WriteLine($"{nameof(MyFunctionFilter)}.FunctionInvoking - {context.Function.PluginName}.{context.Function.Name}");
            await next(context);
            this._output.WriteLine($"{nameof(MyFunctionFilter)}.FunctionInvoked - {context.Function.PluginName}.{context.Function.Name}");
        }
    }

    private sealed class MyPromptFilter(ITestOutputHelper output) : IPromptRenderFilter
    {
        private readonly ITestOutputHelper _output = output;

        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            this._output.WriteLine($"{nameof(MyPromptFilter)}.PromptRendering - {context.Function.PluginName}.{context.Function.Name}");
            await next(context);
            this._output.WriteLine($"{nameof(MyPromptFilter)}.PromptRendered - {context.Function.PluginName}.{context.Function.Name}");

        }
    }
}
