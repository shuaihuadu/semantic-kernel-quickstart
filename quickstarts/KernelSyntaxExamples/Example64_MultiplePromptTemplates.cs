﻿namespace KernelSyntaxExamples;

public class Example64_MultiplePromptTemplates(ITestOutputHelper output) : BaseTest(output)
{
    [RetryTheory(typeof(HttpOperationException))]
    [InlineData("semantic-kernel", "Hello AI, my name is {{$name}}. What is the origin of my name?")]
    [InlineData("handlebars", "Hello AI, my name if {{name}}. What is the origin of my name?")]
    public async Task RunAsync(string templateFormat, string prompt)
    {
        this.WriteLine("======== Example64_MultiplePromptTemplates ========");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        AggregatorPromptTemplateFactory promptTemplateFactory = new(
            new KernelPromptTemplateFactory(),
            new HandlebarsPromptTemplateFactory());

        await RunPromptAsync(kernel, prompt, templateFormat, promptTemplateFactory);
    }

    private async Task RunPromptAsync(Kernel kernel, string prompt, string templateFormat, IPromptTemplateFactory promptTemplateFactory)
    {
        this.WriteLine($"======== {templateFormat} : {prompt} ========");

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
            {"name","Bob" }
        };

        FunctionResult result = await kernel.InvokeAsync(function, arguments);

        this.WriteLine(result.GetValue<string>());
    }
}