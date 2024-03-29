﻿namespace KernelSyntaxExamples;

public class Example66_FunctionCallingStepwisePlanner : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        string[] questions = [
            "What is the current hour number, plus 5?",
            "What is 387 minus 22? Email the solution to Zhangsan and Lisi.",
            "Write a limerick, translate it to Spanish, and send it to Wangwu"
        ];

        Kernel kernel = InitializeKernel();

        FunctionCallingStepwisePlannerOptions options = new FunctionCallingStepwisePlannerOptions
        {
            MaxIterations = 15,
            MaxTokens = 4000
        };

        FunctionCallingStepwisePlanner planner = new(options);

        foreach (string question in questions)
        {
            FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);
            this.WriteLine($"Q: {question} \n A: {result.FinalAnswer}");
        }
    }

    private Kernel InitializeKernel()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.ChatModelId)
            .Build();

        kernel.ImportPluginFromType<EmailPlugin>();
        kernel.ImportPluginFromType<MathPlugin>();
        kernel.ImportPluginFromType<TimePlugin>();

        return kernel;
    }

    public Example66_FunctionCallingStepwisePlanner(ITestOutputHelper output) : base(output)
    {
    }
}
