// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.SemanticKernel.Planning;

namespace Planners;

[TestClass]
public class FunctionCallStepwisePlanning : BaseTest
{
    [TestMethod]
    public async Task RunAsync()
    {
        string[] questions =
        [
            "What is the current hour number, plus 5?",
            //"What is 387 minus 22? Email the solution to John and Mary.",
            //"Write a limerick, translate it to Spanish, and send it to Jane"
        ];

        Kernel kernel = InitializeKernel();

        FunctionCallingStepwisePlannerOptions plannerOptions = new()
        {
            MaxIterations = 15,
            MaxTokens = 1000
        };

        FunctionCallingStepwisePlanner planner = new(plannerOptions);

        foreach (string question in questions)
        {
            FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);

            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Q: {question}");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"ChatHistory: ");
            foreach (var chat in result.ChatHistory ?? [])
            {
                Console.WriteLine($"{chat.Role} : {chat.Content}");
            }
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"A: {result.FinalAnswer}");
        }
    }

    private static Kernel InitializeKernel()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName: TestConfiguration.AzureOpenAI.DeploymentName, endpoint: TestConfiguration.AzureOpenAI.Endpoint, apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        kernel.ImportPluginFromType<DateTimePlugin>();

        return kernel;
    }
}

public class DateTimePlugin
{
    [KernelFunction, Description("获取当前时间")]
    public string LocalNow() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}
