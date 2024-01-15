namespace KernelSyntaxExamples;

public static class Example66_FunctionCallingStepwisePlanner
{
    public static async Task RunAsync()
    {
        string[] questions = [
            "What is the current hour number, plus 5?",
            "What is 387 minus 22? Emial the solution to John and Mary.",
            "Write a limerick, translate it to Spanish, and send it to Wangwu"
        ];

        Kernel kernel = InitializeKernel();

        FunctionCallingStepwisePlannerConfig config = new()
        {
            MaxIterations = 15,
            MaxTokens = 4000
        };

        FunctionCallingStepwisePlanner planner = new(config);

        foreach (string question in questions)
        {
            //TODO Unrecognized request arguments supplied: tool_choice, tools Status: 400(model_error)
            FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);
            Console.WriteLine($"Q: {question} \n A: {result.FinalAnswer}");
        }
    }

    private static Kernel InitializeKernel()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        kernel.ImportPluginFromType<EmailPlugin>();
        kernel.ImportPluginFromType<MathPlugin>();
        kernel.ImportPluginFromType<TimePlugin>();

        return kernel;
    }
}
