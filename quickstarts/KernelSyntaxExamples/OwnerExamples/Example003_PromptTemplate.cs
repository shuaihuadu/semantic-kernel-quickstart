namespace KernelSyntaxExamples.OwnerExamples;

public class Example003_PromptTemplate(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        IPromptTemplateFactory factory = new KernelPromptTemplateFactory();

        IPromptTemplate promptTemplate = factory.Create(new PromptTemplateConfig
        {
            Template = "{{$input}}"
        });

        KernelArguments arguments = new()
        {
            ["input"] = "Hello World!"
        };

        Kernel kernel = new();

        string prompt = await promptTemplate.RenderAsync(kernel, arguments);

        WriteLine(prompt);
    }
}