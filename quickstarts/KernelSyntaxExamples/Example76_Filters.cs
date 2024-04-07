namespace KernelSyntaxExamples;

public class Example76_Filters(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task FunctionAndPromptFiltersAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton(this.Output);

        builder.Services.AddSingleton<IFunctionFilter, FirstFunctionFilter>();
        builder.Services.AddSingleton<IFunctionFilter, SecondFunctionFilter>();

        Kernel kernel = builder.Build();

        kernel.PromptFilters.Add(new FirstPromptFilter(this.Output));

        KernelFunction function = kernel.CreateFunctionFromPrompt("What is Seattle", functionName: "MyFunction");

        FunctionResult result = await kernel.InvokeAsync(function);

        WriteLine(result.ToString());
    }

    private sealed class FirstFunctionFilter : IFunctionFilter
    {
        private readonly ITestOutputHelper _output;

        public FirstFunctionFilter(ITestOutputHelper output)
        {
            this._output = output;
        }

        public void OnFunctionInvoking(FunctionInvokingContext context) => this._output.WriteLine($"{nameof(FirstFunctionFilter)}.{nameof(OnFunctionInvoking)} - {context.Function.Name}");

        public void OnFunctionInvoked(FunctionInvokedContext context) => this._output.WriteLine($"{nameof(FirstFunctionFilter)}.{nameof(OnFunctionInvoked)} - {context.Function.Name}");
    }

    private sealed class SecondFunctionFilter : IFunctionFilter
    {
        private readonly ITestOutputHelper _output;

        public SecondFunctionFilter(ITestOutputHelper output)
        {
            this._output = output;
        }

        public void OnFunctionInvoked(FunctionInvokedContext context) => this._output.WriteLine($"{nameof(SecondFunctionFilter)}.{nameof(OnFunctionInvoked)} - {context.Function.Name}");

        public void OnFunctionInvoking(FunctionInvokingContext context) => this._output.WriteLine($"{nameof(SecondFunctionFilter)}.{nameof(OnFunctionInvoking)} - {context.Function.Name}");
    }

    private sealed class FirstPromptFilter : IPromptFilter
    {
        private readonly ITestOutputHelper _output;

        public FirstPromptFilter(ITestOutputHelper output)
        {
            this._output = output;
        }

        public void OnPromptRendered(PromptRenderedContext context) => this._output.WriteLine($"{nameof(FirstPromptFilter)}.{nameof(OnPromptRendered)} - {context.Function.Name}");

        public void OnPromptRendering(PromptRenderingContext context) => this._output.WriteLine($"{nameof(FirstPromptFilter)}.{nameof(OnPromptRendering)} - {context.Function.Name}");
    }

    private sealed class FuctionFilterExample : IFunctionFilter
    {
        public void OnFunctionInvoked(FunctionInvokedContext context)
        {
            object? value = context.Result.GetValue<object>();

            context.SetResultValue("new result value");

            CompletionsUsage? usage = context.Result.Metadata?["Usage"] as CompletionsUsage;
        }

        public void OnFunctionInvoking(FunctionInvokingContext context)
        {
            context.Arguments["input"] = "new input";

            context.Cancel = true;
        }
    }

    private sealed class PromptFilterExample : IPromptFilter
    {
        public void OnPromptRendered(PromptRenderedContext context)
        {
            context.RenderedPrompt = "Safe prompt";
        }

        public void OnPromptRendering(PromptRenderingContext context)
        {
            string functionName = context.Function.Name;
        }
    }
}