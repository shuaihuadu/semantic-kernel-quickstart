// Copyright (c) IdeaTech. All rights reserved.

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;

namespace Planners;

[TestClass]
public class ReportAgent : BaseTest
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    [TestMethod]
    public async Task GenerateReportV1Async()
    {
        string goal = "获取.NET的相关新闻，写出一个html报告并保存，结果务必使用中文";

        Kernel kernel = this.GetKernel();

        ChatHistory functionCallingChatHistory = [];
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        functionCallingChatHistory.AddUserMessage(goal);

        ChatMessageContent? functionCallingResult = await chatCompletionService.GetChatMessageContentAsync(functionCallingChatHistory, executionSettings, kernel);

        Console.WriteLine($"Auto Function Calling execution result: {functionCallingResult.Content}");
        Console.WriteLine($"Chat history containing the planning process: {JsonSerializer.Serialize(functionCallingChatHistory, _jsonSerializerOptions)}");
    }

    [TestMethod]
    public async Task GenerateReportV2Async()
    {
        string goal = "获取.NET的相关新闻，写出一个html报告并保存，结果务必使用中文";

        Kernel kernel = this.GetKernel();

        FunctionCallingStepwisePlanner planner = new(new FunctionCallingStepwisePlannerOptions
        {
            MaxIterations = 15,
            ExecutionSettings = new AzureOpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            }
        });

        FunctionCallingStepwisePlannerResult plannerResult = await planner.ExecuteAsync(kernel, goal);

        Console.WriteLine($"Planner execution result: {plannerResult.FinalAnswer}");
        Console.WriteLine($"Chat history containing the planning process: {JsonSerializer.Serialize(plannerResult.ChatHistory, _jsonSerializerOptions)}");
    }

    private Kernel GetKernel(bool enableLogging = false)
    {
        var builder = Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName: TestConfiguration.AzureOpenAI.DeploymentName, endpoint: TestConfiguration.AzureOpenAI.Endpoint, apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        if (enableLogging)
        {
            builder.Services.AddSingleton<ILoggerFactory>(this.LoggerFactory);
        }

        var kernel = builder.Build();

        kernel.ImportPluginFromType<FileIOPlugin>();
        kernel.ImportPluginFromType<HttpPlugin>();
        kernel.ImportPluginFromType<DirectoryPlugin>();
        kernel.ImportPluginFromType<NewsPlugin>();
        kernel.ImportPluginFromType<HtmlPlugin>();

        return kernel;
    }

    private sealed class DirectoryPlugin
    {
        [KernelFunction, Description("获取桌面的目录路径")]
        public string GetDesktopDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }

    private sealed class NewsPlugin
    {
        [KernelFunction, Description("获取指定主题或关键字的新闻信息")]
        public List<string> GetNews([Description("需要获取的新闻的主题或者关键字")] string topicOrKeywords)
        {
            List<string> result = [];

            for (int i = 0; i < 10; i++)
            {
                result.Add($"Hello {topicOrKeywords} {i + 1}, This is a mock news of {topicOrKeywords}");
            }

            return result;
        }
    }

    private sealed class HtmlPlugin
    {
        [KernelFunction, Description("编写html报告信息")]
        public async Task<string> GenerateReportAsync([Description("需要写入html的新闻内容")] string content)
        {
            Kernel kernel = Kernel
                .CreateBuilder()
                .AddAzureOpenAIChatCompletion(deploymentName: TestConfiguration.AzureOpenAI.DeploymentName, endpoint: TestConfiguration.AzureOpenAI.Endpoint, apiKey: TestConfiguration.AzureOpenAI.ApiKey)
                .Build();

            KernelArguments arguments = [];

            arguments.Add("content", content);

            FunctionResult result = await kernel.InvokePromptAsync("请分析{{$content}}的内容，并根据内容自动排版，写出较为美观的html，只需输出纯净的完整的html代码即可，不要输出其他无关内容；根据内容，请使用一些效果，优先使用bootstrap，譬如分页、slide模式等常用的用户体验较好的html交互设计", arguments);

            return result.ToString();
        }
    }
}
