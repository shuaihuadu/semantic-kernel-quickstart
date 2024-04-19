namespace KernelSyntaxExamples;

public class Example10_DescribeAllPluginsAndFunctions(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public Task RunAsync()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        kernel.ImportPluginFromType<StaticTextPlugin>();

        kernel.ImportPluginFromType<TextPlugin>("AnotherTextPlugin");

        string folder = RepoFiles.SamplePluginsPath();
        kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "SummarizePlugin"));

        KernelFunction function1 = kernel.CreateFunctionFromPrompt("tell a joke about {{$input}}", new OpenAIPromptExecutionSettings { MaxTokens = 150 });

        KernelFunction function2 = kernel.CreateFunctionFromPrompt(
             "write a novel about {{$input}} in {{$language}} language",
             new OpenAIPromptExecutionSettings { MaxTokens = 150 },
             functionName: "Novel",
             description: "Write a bedtime story");

        IList<KernelFunctionMetadata> kernelFunctionMetadatas = kernel.Plugins.GetFunctionsMetadata();

        this.WriteLine("**********************************************");
        this.WriteLine("****** Registered plugins and functions ******");
        this.WriteLine("**********************************************");
        this.WriteLine();

        foreach (KernelFunctionMetadata kernelFunctionMetadata in kernelFunctionMetadatas)
        {
            PrintFunction(kernelFunctionMetadata);
        }

        return Task.CompletedTask;
    }

    private void PrintFunction(KernelFunctionMetadata kernelFunctionMetadata)
    {
        this.WriteLine($"Plugin: {kernelFunctionMetadata.PluginName}");
        this.WriteLine($"    {kernelFunctionMetadata.Name}: {kernelFunctionMetadata.Description}");

        if (kernelFunctionMetadata.Parameters.Count > 0)
        {
            this.WriteLine("     Params:");

            foreach (var parameter in kernelFunctionMetadata.Parameters)
            {
                this.WriteLine($"      - {parameter.Name}: {parameter.Description}");
                this.WriteLine($"        default: '{parameter.DefaultValue}'");
            }
        }

        this.WriteLine();
    }
}
