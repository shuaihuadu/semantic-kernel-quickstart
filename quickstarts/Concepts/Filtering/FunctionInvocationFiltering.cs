namespace Filtering;

public class FunctionInvocationFiltering(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task FunctionAndPromptFiltersAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton(this.Output);

        builder.Services.AddSingleton<IFunctionInvocationFilter, FirstFunctionInvocationFilter>();
        builder.Services.AddSingleton<IFunctionInvocationFilter, SecondFunctionInvocationFilter>();

        Kernel kernel = builder.Build();

        KernelFunction function = kernel.CreateFunctionFromPrompt("What is Seattle", functionName: "MyFunction");

        kernel.Plugins.Add(KernelPluginFactory.CreateFromFunctions("MyPlugin", functions: [function]));

        FunctionResult result = await kernel.InvokeAsync(kernel.Plugins["MyPlugin"]["MyFunction"]);

        Console.WriteLine(result);
    }

    [Fact]
    public async Task FunctionFilterResultOverrideAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton<IFunctionInvocationFilter, FunctionInvocationFilterExample>();

        Kernel kernel = builder.Build();

        KernelFunction function = KernelFunctionFactory.CreateFromMethod(() => "Result from method");

        FunctionResult result = await kernel.InvokeAsync(function);

        Console.WriteLine(result);

        Console.WriteLine($"Metadata: {string.Join(",", result.Metadata!.Select(kv => $"{kv.Key}:{kv.Value}"))}");
    }

    [Fact]
    public async Task FunctionFilterResultOverrideOnStreamingAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton<IFunctionInvocationFilter, StreamingFunctionInvocationFilterExample>();

        Kernel kernel = builder.Build();

        static IEnumerable<int> GetData()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

        KernelFunction function = KernelFunctionFactory.CreateFromMethod(GetData);

        await foreach (var item in kernel.InvokeStreamingAsync<int>(function))
        {
            Console.WriteLine(item);
        }
    }

    [Fact]
    public async Task FunctionFilterExceptionHandlingAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton<IFunctionInvocationFilter>(new ExceptionHandlingFilterExample(NullLogger.Instance));

        Kernel kernel = builder.Build();

        KernelFunction function = KernelFunctionFactory.CreateFromMethod(() => { throw new KernelException("Exception in function"); });

        FunctionResult result = await kernel.InvokeAsync(function);

        Console.WriteLine(result);
    }

    [Fact]
    public async Task FunctionFilterExceptionHandlingOnStreamingAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton<IFunctionInvocationFilter>(new StreamingExceptionHandlingFilterExample(NullLogger.Instance));

        Kernel kernel = builder.Build();

        static IEnumerable<string> GetData()
        {
            yield return "first chunk";

            throw new KernelException("Exception in function");
        }

        KernelFunction function = KernelFunctionFactory.CreateFromMethod(GetData);

        await foreach (var item in kernel.InvokeStreamingAsync<string>(function))
        {
            Console.WriteLine(item);
        }
    }

    private class FunctionInvocationFilterExample : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            context.Arguments["input"] = "new input";

            await next(context);

            object? value = context.Result!.GetValue<object>();

            object? usage = context.Result.Metadata?["Usage"];

            Dictionary<string, object?> metadata = context.Result.Metadata is not null ? new(context.Result.Metadata) : [];

            metadata["metadata_key"] = "metadata_value";

            context.Result = new FunctionResult(context.Result, "Result from filter")
            {
                Metadata = metadata
            };
        }
    }

    private class FirstFunctionInvocationFilter(ITestOutputHelper output) : IFunctionInvocationFilter
    {
        private readonly ITestOutputHelper _output = output;

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            this._output.WriteLine($"{nameof(FirstFunctionInvocationFilter)}.FunctionInvoking - {context.Function.PluginName}.{context.Function.Name}");

            await next(context);

            this._output.WriteLine($"{nameof(FirstFunctionInvocationFilter)}.FunctionInvoked - {context.Function.PluginName}.{context.Function.Name}");
        }
    }

    private class SecondFunctionInvocationFilter(ITestOutputHelper output) : IFunctionInvocationFilter
    {
        private readonly ITestOutputHelper _output = output;
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            this._output.WriteLine($"{nameof(SecondFunctionInvocationFilter)}.FunctionInvoking - {context.Function.PluginName}.{context.Function.Name}");

            await next(context);

            this._output.WriteLine($"{nameof(SecondFunctionInvocationFilter)}.FunctionInvoked - {context.Function.PluginName}.{context.Function.Name}");
        }
    }

    private class StreamingFunctionInvocationFilterExample : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            await next(context);

            IAsyncEnumerable<int>? enumerable = context.Result.GetValue<IAsyncEnumerable<int>>();

            context.Result = new FunctionResult(context.Result, OverrideStreamingDataAsync(enumerable!));
        }

        private async IAsyncEnumerable<int> OverrideStreamingDataAsync(IAsyncEnumerable<int> data)
        {
            await foreach (var item in data)
            {
                yield return item * 2;
            }
        }
    }

    private class ExceptionHandlingFilterExample(ILogger logger) : IFunctionInvocationFilter
    {
        private readonly ILogger _logger = logger;

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Somthing went wrong during function invocation");

                context.Result = new FunctionResult(context.Result, "Friendly message instead of exception");
            }
        }
    }

    private class StreamingExceptionHandlingFilterExample(ILogger logger) : IFunctionInvocationFilter
    {
        private readonly ILogger _logger = logger;

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            await next(context);

            IAsyncEnumerable<string>? enumerable = context.Result.GetValue<IAsyncEnumerable<string>>();

            context.Result = new FunctionResult(context.Result, StreamingWithExceptionHandlingAsync(enumerable!));
        }

        private async IAsyncEnumerable<string> StreamingWithExceptionHandlingAsync(IAsyncEnumerable<string> data)
        {
            IAsyncEnumerator<string> enumerator = data.GetAsyncEnumerator();

            await using (enumerator.ConfigureAwait(false))
            {
                while (true)
                {
                    string result;

                    try
                    {
                        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            break;
                        }

                        result = enumerator.Current;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "Something went wrong during function invocation");

                        result = "chunk instead of exception";
                    }

                    yield return result;
                }
            }
        }
    }
}
