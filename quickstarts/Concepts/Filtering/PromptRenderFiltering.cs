namespace Filtering;

public class PromptRenderFiltering(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task FunctionAndPromptFiltersAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton(this.Output);

        Kernel kernel = builder.Build();

        kernel.PromptRenderFilters.Add(new FirstPromptFilter(this.Output));

        KernelFunction function = kernel.CreateFunctionFromPrompt("What is Seattle", functionName: "MyFunction");

        kernel.Plugins.Add(KernelPluginFactory.CreateFromFunctions("MyPlugin", functions: [function]));

        FunctionResult result = await kernel.InvokeAsync(kernel.Plugins["MyPlugin"]["MyFunction"]);

        Console.WriteLine(result.ToString());
    }

    [Fact]
    public async Task PromptFilterRenderedPromptOverrideAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton<IPromptRenderFilter, PromptFilterExample>();

        Kernel kernel = builder.Build();

        FunctionResult result = await kernel.InvokePromptAsync("Hi, how can you help me?");

        Console.WriteLine(result);
    }

    private sealed class FirstPromptFilter(ITestOutputHelper output) : IPromptRenderFilter
    {
        private readonly ITestOutputHelper _output = output;

        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            this._output.WriteLine($"{nameof(FirstPromptFilter)}.PromptRendering - {context.Function.PluginName}.{context.Function.Name}");

            await next(context);

            this._output.WriteLine($"{nameof(FirstPromptFilter)}.PromptRendered - {context.Function.PluginName}.{context.Function.Name}");
        }
    }

    private sealed class PromptFilterExample : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            string functionName = context.Function.Name;

            await next(context);

            context.RenderedPrompt = "Respond with following text: Prompt from filter.";
        }
    }
}