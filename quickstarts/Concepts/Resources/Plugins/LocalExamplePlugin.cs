namespace Resources.Plugins;

internal sealed class LocalExamplePlugin
{
    [KernelFunction]
    public void NoInputWithVoidResult()
    {
        Console.WriteLine($"Running {nameof(NoInputWithVoidResult)} -> No input");
    }

    [KernelFunction]
    public Task NotInputTaskWithVoidResult()
    {
        Console.WriteLine($"Running {nameof(NotInputTaskWithVoidResult)} -> No input");
        return Task.CompletedTask;
    }

    [KernelFunction]
    public string InputDateTimeWithStringResult(DateTime currentDate)
    {
        string result = currentDate.ToString(CultureInfo.InvariantCulture);

        Console.WriteLine($"Running {nameof(InputDateTimeWithStringResult)} -> [currentDate = {currentDate}] -> result: {result}");

        return result;
    }

    [KernelFunction]
    public Task<string> NoInputTaskWithStringResult()
    {
        string result = "string result";

        Console.WriteLine($"Running {nameof(NoInputTaskWithStringResult)} -> No input -> result: {result}");

        return Task.FromResult(result);
    }

    [KernelFunction]
    public void MultipleInputsWithVoidResult(string x, int y, double z)
    {
        Console.WriteLine($"Running {nameof(MultipleInputsWithVoidResult)} -> input: [x = {x}, y = {y}], z = {z}");
    }

    [KernelFunction]
    public string ComplexInputWithStringResult(object complexObject)
    {
        string result = complexObject.GetType().Name;

        Console.WriteLine($"Running {nameof(ComplexInputWithStringResult)} -> input: [complexObject = {complexObject}] -> result: {result}");

        return result;
    }

    [KernelFunction]
    public Task<string> InputStringTaskWithStringResult(string echoInput)
    {
        Console.WriteLine($"Running {nameof(InputStringTaskWithStringResult)} -> input: [echoInput = {echoInput}] -> result: {echoInput}");

        return Task.FromResult(echoInput);
    }

    [KernelFunction]
    public Task InputStringTaskWithVoidResult(string x)
    {
        Console.WriteLine($"Running {nameof(InputStringTaskWithVoidResult)} -> input: [x = {x}]");

        return Task.FromResult(x);
    }

    [KernelFunction]
    public FunctionResult NoInputWithFunctionResult()
    {
        KernelFunction internalFunction = KernelFunctionFactory.CreateFromMethod(() => { });

        FunctionResult result = new(internalFunction);

        Console.WriteLine($"Running {nameof(NoInputWithFunctionResult)} -> No input -> result: {result.GetType().Name}");

        return result;
    }

    [KernelFunction]
    public async Task<FunctionResult> NoInputTaskWithFunctionResult(Kernel kernel)
    {
        FunctionResult result = await kernel.InvokeAsync(kernel.Plugins["Examples"][nameof(NoInputWithVoidResult)]);

        Console.WriteLine($"Running {nameof(NoInputTaskWithFunctionResult)} -> Injected kernel -> result: {result.GetType().Name}");

        return result;
    }

    [KernelFunction]
    public async Task<string> TaskInjectingKernelWithInputTextAndStringResult(Kernel kernel, string textToSummarize)
    {
        string? summary = await kernel.InvokeAsync<string>(kernel.Plugins["SummarizePlugin"]["Summarize"], new() { ["input"] = textToSummarize });

        Console.WriteLine($"Running {nameof(TaskInjectingKernelWithInputTextAndStringResult)} -> Injected kernel + input: [textToSummarize: {textToSummarize[..15]}...{textToSummarize[^15..]}] -> result: {summary}");

        return summary!;
    }

    [KernelFunction, Description("Example function injecting itself as a parameter")]
    public Task<string> TaskInjectingKernelFunctionWithStringResult(KernelFunction executingFunction)
    {
        string result = $"Name: {executingFunction.Name}, Description: {executingFunction.Description}";

        Console.WriteLine($"Running {nameof(this.TaskInjectingKernelFunctionWithStringResult)} -> Injected Function -> result: {result}");

        return Task.FromResult(result);
    }

    [KernelFunction]
    public Task TaskInjectingLoggerWithNoResult(ILogger logger)
    {
        logger.LogWarning("Running {FunctionName} -> Injected Logger", nameof(TaskInjectingLoggerWithNoResult));

        Console.WriteLine($"Running {nameof(TaskInjectingLoggerWithNoResult)}");

        return Task.CompletedTask;
    }

    [KernelFunction]
    public Task TaskInjectingLoggerFactoryWithNoResult(ILoggerFactory loggerFactory)
    {
        loggerFactory.CreateLogger<LocalExamplePlugin>()
            .LogWarning("Running {FunctionName} -> Injected LoggerFactory", nameof(TaskInjectingLoggerFactoryWithNoResult));

        Console.WriteLine($"Running {nameof(TaskInjectingLoggerFactoryWithNoResult)} -> Injected LoggerFactory");

        return Task.CompletedTask;
    }

    [KernelFunction]
    public async Task<string> TaskInjectingServiceSelectorWithStringResult(
        Kernel kernel,
        KernelFunction kernelFunction,
        KernelArguments arguments,
        IAIServiceSelector serviceSelector)
    {
        ChatMessageContent? chatMessageContent = null;

        if (serviceSelector.TrySelectAIService(
            kernel,
            kernelFunction,
            arguments,
            out IChatCompletionService? chatCompletionService,
            out PromptExecutionSettings? executionSettings))
        {
            chatMessageContent = await chatCompletionService.GetChatMessageContentAsync(new ChatHistory("How much is 5 + 5 ?"), executionSettings);
        }

        string? result = chatMessageContent?.Content;

        Console.WriteLine($"Running {nameof(TaskInjectingServiceSelectorWithStringResult)} -> Injected Kernel, KernelFunction, KernelArguments, Service Selector -> result: {result}");

        return result ?? string.Empty;
    }

    [KernelFunction]
    public Task<string> TaskInjectingCultureInfoOrIFormatProviderWithStringResult(CultureInfo cultureInfo, IFormatProvider formatProvider)
    {
        string result = $"Culture Name: {cultureInfo.Name}, FormatProvider Equals CultureInfo?: {formatProvider.Equals(cultureInfo)}";

        Console.WriteLine($"Running {nameof(TaskInjectingCultureInfoOrIFormatProviderWithStringResult)} -> Injected CultureInfo, IFormateProvider -> result: {result}");

        return Task.FromResult(result);
    }

    [KernelFunction]
    public Task<string> TaskInjectingCancellationTokenWithStringResult(CancellationToken cancellationToken)
    {
        string result = $"Cancellation requested: {cancellationToken.IsCancellationRequested}";

        Console.WriteLine($"Running {nameof(TaskInjectingCancellationTokenWithStringResult)} -> Injected Cancellation Token -> result: {result}");

        return Task.FromResult(result);
    }

    public override string ToString()
    {
        return "Complex type result ToString override";
    }
}
