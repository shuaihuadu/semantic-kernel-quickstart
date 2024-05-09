namespace Filtering;

public class AutoFunctionInvocationFiltering(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task AutoFunctionInvocationFilterAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton<IAutoFunctionInvocationFilter>(new AutoFunctionInvocationFilterExample(this.Output));

        Kernel kernel = builder.Build();

        KernelFunction function = KernelFunctionFactory.CreateFromMethod(() => "Result from function", "MyFunction");

        kernel.ImportPluginFromFunctions("MyFunction", [function]);

        OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.RequireFunction(function.Metadata.ToOpenAIFunction(), autoInvoke: true)
        };

        FunctionResult result = await kernel.InvokePromptAsync("Invoke provided function and return result", new(executionSettings));

        WriteLine(result);
    }

    private sealed class AutoFunctionInvocationFilterExample(ITestOutputHelper output) : IAutoFunctionInvocationFilter
    {
        private readonly ITestOutputHelper _output = output;

        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            string functionName = context.Function.Name;

            ChatHistory chatHistory = context.ChatHistory;

            IEnumerable<FunctionCallContent> functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last());

            this._output.WriteLine($"Request sequence index: {context.RequestSequenceIndex}");

            this._output.WriteLine($"Function sequence index: {context.FunctionSequenceIndex}");

            this._output.WriteLine($"Total number of functions: {context.FunctionCount}");

            await next(context);

            FunctionResult result = context.Result;

            context.Result = new FunctionResult(context.Result, "Result from auto function invocation filter");

            context.Terminate = true;
        }
    }
}
