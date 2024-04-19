
namespace KernelSyntaxExamples;

public class Example09_FunctionTypes(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Method Function types ========");

        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddLogging(
            configure => configure.AddConsole()
            .SetMinimumLevel(LogLevel.Warning));

        Kernel kernel = builder.Build();
        kernel.Culture = new CultureInfo("zh-CN");

        KernelPlugin plugin = kernel.ImportPluginFromType<LocalExamplePlugin>("Examples");

        string folder = RepoFiles.SamplePluginsPath();

        kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "SummarizePlugin"));

        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.NoInputWithVoidResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.NotInputTaskWithVoidResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.InputDateTimeWithStringResult)], new() { ["currentDate"] = DateTime.Now });
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.NoInputTaskWithStringResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.MultipleInputsWithVoidResult)], new() { ["x"] = "x string", ["y"] = 1988, ["z"] = 0.9 });
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.ComplexInputWithStringResult)], new() { ["complexObject"] = new LocalExamplePlugin() });
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.InputStringTaskWithStringResult)], new() { ["echoInput"] = "return this" });
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.InputStringTaskWithVoidResult)], new() { ["x"] = "x string" });
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.NoInputWithFunctionResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.NoInputTaskWithFunctionResult)]);

        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.TaskInjectingKernelWithInputTextAndStringResult)],
            new()
            {
                ["textToSummarize"] = @"C# is a modern, versatile language by Microsoft, blending the efficiency of C++ 
                                            with Visual Basic's simplicity. It's ideal for a wide range of applications, 
                                            emphasizing type safety, modularity, and modern programming paradigms."
            });
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.TaskInjectingKernelFunctionWithStringResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.TaskInjectingLoggerWithNoResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.TaskInjectingLoggerFactoryWithNoResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.TaskInjectingServiceSelectorWithStringResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.TaskInjectingCultureInfoOrIFormatProviderWithStringResult)]);
        await kernel.InvokeAsync(plugin[nameof(LocalExamplePlugin.TaskInjectingCancellationTokenWithStringResult)]);

        await kernel.InvokeAsync(kernel.Plugins["Examples"][nameof(LocalExamplePlugin.NoInputWithVoidResult)]);
    }
}