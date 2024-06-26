﻿namespace Functions;

public class PromptFunctions_Inline(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Inline Function Definition ========");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        string promptTemplate = @"
Generate a creative reason or excuse for the given event.
Be creative and be funny. Let your imagination run wild.

Event: I am running late.
Excuse: I was being held ransom by giraffe gangsters.

Event: I haven't been to the gym for a year
Excuse: I've been too busy training my pet dragon.

Event: {{$input}}
";

        KernelFunction excuseFunction = kernel.CreateFunctionFromPrompt(promptTemplate, new PromptExecutionSettings
        {
            ModelId = TestConfiguration.AzureOpenAIConfig.ModelId,
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 100,
                ["temperature"] = 0.0,
                ["top_p"] = 1
            }
        });

        FunctionResult result = await kernel.InvokeAsync(excuseFunction, new() { ["input"] = "I missed the F1 final race" });
        this.WriteLine(result.GetValue<string>());

        result = await kernel.InvokeAsync(excuseFunction, new() { ["input"] = "sorry I forgot your birthday" });
        this.WriteLine(result.GetValue<string>());

        var fixedFunction = kernel.CreateFunctionFromPrompt($"Translate this data {DateTimeOffset.Now:f} to French format", new PromptExecutionSettings
        {
            ModelId = TestConfiguration.AzureOpenAIConfig.ModelId,
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 50,
                ["temperature"] = 0.0,
                ["top_p"] = 1
            }
        });

        result = await kernel.InvokeAsync(fixedFunction);
        this.WriteLine(result.GetValue<string>());
    }
}
