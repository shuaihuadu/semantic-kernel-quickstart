using Microsoft.SemanticKernel.Plugins.Memory;

namespace KernelSyntaxExamples;

public class Example78_RAG : BaseTest
{
    [Fact]
    public async Task RAGWithCustomPluginAsync()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        kernel.ImportPluginFromType<CustomPlugin>();

        FunctionResult result = await kernel.InvokePromptAsync("{{search 'budget by year'}} What is my budget for 2024?");

        WriteLine(result);
    }

    [Fact(Skip = "Requieres Chroma server up and running")]
    public async Task RAGWithTextMemoryPluginAsync()
    {
        ISemanticTextMemory memory = new MemoryBuilder()
            .WithMemoryStore(new ChromaMemoryStore("http://localhost:8000"))
            .WithOpenAITextEmbeddingGeneration(
                //deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                //endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                //apiKey: TestConfiguration.AzureOpenAI.ApiKey
                modelId: string.Empty,
                 apiKey: string.Empty)
            .Build();


        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        kernel.ImportPluginFromObject(new TextMemoryPlugin(memory));

        FunctionResult result = await kernel.InvokePromptAsync("{{recall 'budget by year' collection='finances'}} Waht is my budget for 2024?");

        WriteLine(result);
    }

    private sealed class CustomPlugin
    {
        [KernelFunction]
        public string SearchAsync(string query)
        {
            return "Year Budget 2020 100,000 2021 120,000 2022 150,000 2023 200,000 2024 364,000";
        }
    }

    public Example78_RAG(ITestOutputHelper output) : base(output)
    {
    }
}
