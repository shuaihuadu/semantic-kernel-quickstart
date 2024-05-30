namespace PromptTemplates;

public class MultiplePromptTemplates(ITestOutputHelper output) : BaseTest(output)
{
    [RetryTheory(typeof(HttpOperationException))]
    [InlineData("semantic-kernel", "Hello AI, my name is {{$name}}. What is the origin of my name?", "Paz")]
    [InlineData("handlebars", "Hello AI, my name if {{name}}. What is the origin of my name?", "Mira")]
    [InlineData("liquid", "Hello AI, my name is {{name}}. What is the origin of my name?", "Aoibhinn")]
    public Task InvokeDifferentPromptTypes(string templateFormat, string prompt, string name)
    {
        Console.WriteLine($"======== {nameof(MultiplePromptTemplates)} ========");

        Kernel kernel = KernelHelper.CreateKernelWithAzureOpenAIChatCompletion();

        AggregatorPromptTemplateFactory promptTemplateFactory = new(
            new KernelPromptTemplateFactory(),
            new HandlebarsPromptTemplateFactory(),
            new LiquidPromptTemplateFactory());

        return RunPromptAsync(kernel, prompt, name, templateFormat, promptTemplateFactory);
    }

    private async Task RunPromptAsync(Kernel kernel, string prompt, string name, string templateFormat, IPromptTemplateFactory promptTemplateFactory)
    {
        Console.WriteLine($"======== {templateFormat} : {prompt} ========");

        KernelFunction function = kernel.CreateFunctionFromPrompt(
            promptConfig: new PromptTemplateConfig
            {
                Template = prompt,
                TemplateFormat = templateFormat,
                Name = "MyFunction"
            },
            promptTemplateFactory: promptTemplateFactory);

        KernelArguments arguments = new()
        {
            {"name",name }
        };

        FunctionResult result = await kernel.InvokeAsync(function, arguments);

        Console.WriteLine(result.GetValue<string>());
    }
}