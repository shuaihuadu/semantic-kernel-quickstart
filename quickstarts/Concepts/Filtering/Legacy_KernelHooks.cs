namespace Filtering;

#pragma warning disable CS0618 // Events are deprecated

public class Legacy_KernelHooks(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task GetUsageAsync()
    {
        this.WriteLine("\n======== Get Usage Data ========\n");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        const string FunctionPrompt = "Write a random paragraph about: {{$input}}.";

        KernelFunction excuseFunction = kernel.CreateFunctionFromPrompt(
            FunctionPrompt,
            functionName: "Excuse",
            executionSettings: new OpenAIPromptExecutionSettings
            {
                MaxTokens = 100,
                Temperature = 0.4,
                TopP = 1
            });

        void PreHandler(object? sender, FunctionInvokingEventArgs e)
        {
            this.WriteLine($"{e.Function.Name} : Pre Execution Handler - Triggered");
        }

        void RemovedPreExecutionHandler(object? sender, FunctionInvokingEventArgs e)
        {
            this.WriteLine($"{e.Function.Name} : Pre Execution Handler - Should not trigger");
            e.Cancel = true;
        }

        void PostExecutionHandler(object? sender, FunctionInvokedEventArgs e)
        {
            this.WriteLine($"{e.Function.Name} : Post Execution Handler - Usage: {e.Result.Metadata?["Usage"]?.AsJson()}");
        }

        kernel.FunctionInvoking += PreHandler;
        kernel.FunctionInvoked += PostExecutionHandler;

        kernel.FunctionInvoking += RemovedPreExecutionHandler;
        kernel.FunctionInvoking -= RemovedPreExecutionHandler;

        const string Input = "I missed the F1 final race";

        FunctionResult result = await kernel.InvokeAsync(excuseFunction, new() { ["input"] = Input });

        this.WriteLine($"Function Result:{result.GetValue<string>()}");
    }


    [Fact]
    public async Task GetRenderedPromptAsync()
    {
        this.WriteLine("\n======== Get Rendered Prompt ========\n");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        const string FunctionPrompt = "Write a random paragraph about: {{$input}} in the style of {{$style}}";

        KernelFunction excuseFunction = kernel.CreateFunctionFromPrompt(
            FunctionPrompt,
            functionName: "Excuse",
            executionSettings: new OpenAIPromptExecutionSettings
            {
                MaxTokens = 100,
                Temperature = 0.4,
                TopP = 1
            });

        void PromptRendering(object? sender, PromptRenderingEventArgs e)
        {
            this.WriteLine($"{e.Function.Name} : Prompt Rendering Handler - Triggered");
            e.Arguments["style"] = "Seinfeld";
        }

        void PromptRendered(object? sender, PromptRenderedEventArgs e)
        {
            this.WriteLine($"{e.Function.Name} : Prompt Rendered Handler - Triggered");
            e.RenderedPrompt += "USE SHORT, CLEAR, COMPLETE SENTENCES.";
        }

        kernel.PromptRendering += PromptRendering;
        kernel.PromptRendered += PromptRendered;

        const string Input = "I missed the F1 final race";
        FunctionResult result = await kernel.InvokeAsync(excuseFunction, new() { ["input"] = Input });

        this.WriteLine($"Function Result: {result.GetValue<string>()}");
    }


    [Fact]
    public async Task ChangingResultAsync()
    {
        this.WriteLine("\n======== Changing/Filtering Function Result ========\n");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        const string FunctionPrompt = "Write a paragraph about Handlers.";

        KernelFunction writerFunction = kernel.CreateFunctionFromPrompt(
            FunctionPrompt,
            functionName: "Writer",
            executionSettings: new OpenAIPromptExecutionSettings() { MaxTokens = 100, Temperature = 0.4, TopP = 1 });

        void ChangeDataHandler(object? sender, FunctionInvokedEventArgs e)
        {
            string originalOutput = e.Result.GetValue<string>()!;

            string newOutput = Regex.Replace(originalOutput, "[aeiouAEIOU0-9]", "*");

            e.SetResultValue(newOutput);
        }

        kernel.FunctionInvoked += ChangeDataHandler;

        FunctionResult result = await kernel.InvokeAsync(writerFunction);

        this.WriteLine($"Function Result: {result.GetValue<string>()}");
    }


    [Fact]
    public async Task BeforeInvokeCancellationiAsync()
    {
        this.WriteLine("\n======== Cancelling Pipeline Execution - Invoking event ========\n");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        const string FunctionPrompt = "Write a paragraph about: Cancellation.";

        KernelFunction writerFunction = kernel.CreateFunctionFromPrompt(
            FunctionPrompt,
            functionName: "Writer",
            executionSettings: new OpenAIPromptExecutionSettings() { MaxTokens = 1000, Temperature = 1, TopP = 0.5 });

        kernel.FunctionInvoking += (object? sender, FunctionInvokingEventArgs e) =>
        {
            this.WriteLine($"{e.Function.Name} : FunctionInvoking - Cancelling before execution");
            e.Cancel = true;
        };

        int functionInvokeCount = 0;

        kernel.FunctionInvoked += (object? sender, FunctionInvokedEventArgs e) =>
        {
            functionInvokeCount++;
        };

        FunctionResult result = await kernel.InvokeAsync(writerFunction);

        this.WriteLine($"Function Invocation Times: {functionInvokeCount}");
    }


    [Fact]
    public async Task AfterInvokeCancellationAsync()
    {
        this.WriteLine("\n======== Cancelling Pipeline Execution - Invoked event ========\n");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        int functionInvokingCount = 0;
        int functionInvokeCount = 0;

        KernelFunction function1 = kernel.CreateFunctionFromPrompt("Write a phrase with Invoke.", functionName: "InvokePhrase");
        KernelFunction function2 = kernel.CreateFunctionFromPrompt("Write a phrase with Cancellation.", functionName: "CancellationPhrase");

        kernel.FunctionInvoking += (object? sender, FunctionInvokingEventArgs e) =>
        {
            functionInvokingCount++;
        };

        kernel.FunctionInvoked += (object? sender, FunctionInvokedEventArgs e) =>
        {
            functionInvokeCount++;
            e.Cancel = true;
        };

        FunctionResult result = await kernel.InvokeAsync(function1);

        this.WriteLine($"Function Invoked Times: {functionInvokeCount}");
        this.WriteLine($"Function Invoking Times:{functionInvokingCount}");
    }
}