// Copyright (c) IdeaTech. All rights reserved.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Planning;
using OpenAI.Chat;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace Planners;

[TestClass]
public class AutoFunctionCallingPlanning : BaseTest
{
    private const string Goal = "Check current UTC time and return current weather in Boston city.";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    [TestMethod]
    public async Task GeneratePlanAsync()
    {
        Kernel kernel = this.GetKernel();
        FunctionCallingStepwisePlanner planner = new(new FunctionCallingStepwisePlannerOptions
        {
            MaxIterations = 1
        });
        FunctionCallingStepwisePlannerResult? plannerResult = await planner.ExecuteAsync(kernel, Goal);
        Console.WriteLine($"Planning: {JsonSerializer.Serialize(plannerResult.ChatHistory, _jsonSerializerOptions)}");
    }

    [TestMethod]
    public async Task SideBySideComparisonWithStepwisePlannerAsync()
    {
        Kernel kernel = this.GetKernel();

        FunctionCallingStepwisePlanner planner = new();
        FunctionCallingStepwisePlannerResult? plannerResult = await planner.ExecuteAsync(kernel, Goal);

        Console.WriteLine($"Planner execution result: {plannerResult.FinalAnswer}");
        Console.WriteLine($"Chat history containing the planning process: {JsonSerializer.Serialize(plannerResult.ChatHistory, _jsonSerializerOptions)}");
        Console.WriteLine($"Planner execution tokens: {GetChatHistoryTokens(plannerResult.ChatHistory)}");

        ChatHistory functionCallingChatHistory = [];
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        functionCallingChatHistory.AddUserMessage(Goal);

        ChatMessageContent? functionCallingResult = await chatCompletionService.GetChatMessageContentAsync(functionCallingChatHistory, executionSettings, kernel);

        Console.WriteLine($"Auto Function Calling execution result: {functionCallingResult.Content}");
        Console.WriteLine($"Chat history containing the planning process: {JsonSerializer.Serialize(functionCallingChatHistory, _jsonSerializerOptions)}");
        Console.WriteLine($"Auto Function Calling execution tokens: {GetChatHistoryTokens(functionCallingChatHistory)}");

        plannerResult = await planner.ExecuteAsync(kernel, Goal, [.. plannerResult.ChatHistory!.Take(..^2)]);
        Console.WriteLine($"Planner re-execution result: {plannerResult.FinalAnswer}");

        functionCallingResult = await chatCompletionService.GetChatMessageContentAsync(functionCallingChatHistory, executionSettings, kernel);
        Console.WriteLine($"Auto Function Calling re-execution result: {functionCallingResult.Content}");
    }

    [TestMethod]
    public async Task PlanExecutionOptionsAsync()
    {
        Kernel kernel = this.GetKernel();

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        FunctionResult kernelResult = await kernel.InvokePromptAsync(Goal, new(executionSettings));

        Console.WriteLine($"Kernel result: {kernelResult}");

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory chatHistory = [];

        chatHistory.AddUserMessage(Goal);

        ChatMessageContent chatCompletionServiceResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);

        Console.WriteLine($"Chat completion service result: {chatCompletionServiceResult.Content}");
        Console.WriteLine($"Chat history containing the planning process: {JsonSerializer.Serialize(chatHistory, _jsonSerializerOptions)}");
    }

    [TestMethod]
    public async Task TelemetryForPlanGenerationAndExecutionAsync()
    {
        Kernel kernel = this.GetKernel(enableLogging: true);

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        FunctionResult result = await kernel.InvokePromptAsync(Goal, new(executionSettings));

        Console.WriteLine($"Kernel result: {result}");
    }

    [TestMethod]
    public async Task PlanCachingForReusabilityAsync()
    {
        Kernel kernel = this.GetKernel();
        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        IChatCompletionService chatCompletionService = new CachedChatCompletionService(kernel.GetRequiredService<IChatCompletionService>());

        Console.WriteLine("First run:");

        ChatHistory firstChatHistory = new([new ChatMessageContent(AuthorRole.User, Goal)]);

        ChatMessageContent chatCompletionServiceResult = await ExecuteWithStopwatchAsync(() => chatCompletionService.GetChatMessageContentAsync(firstChatHistory, executionSettings, kernel));

        Console.WriteLine($"Plan execution result: {chatCompletionServiceResult.Content}");

        Console.WriteLine("Second run:");

        ChatHistory secondChatHistory = new([new ChatMessageContent(AuthorRole.User, Goal)]);

        chatCompletionServiceResult = await ExecuteWithStopwatchAsync(() => chatCompletionService.GetChatMessageContentAsync(secondChatHistory, executionSettings, kernel));

        Console.WriteLine($"Plan execution result: {chatCompletionServiceResult.Content}");
    }

    [TestMethod]
    public async Task UsingFiltersToControlPlanExecutionAsync()
    {
        Kernel kernel = this.GetKernel();

        kernel.FunctionInvocationFilters.Add(new PlanExecutionFilter());

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        FunctionResult result = await kernel.InvokePromptAsync(Goal, new(executionSettings));

        Console.WriteLine($"Kernel result: {result}");
    }

    /// <summary>
    /// Filter to control plan execution and each step (function).
    /// With filters it's possible to observe which step is going to be executed and its arguments, handle exceptions, override step result.
    /// </summary>
    private sealed class PlanExecutionFilter : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            await next(context);

            // For GetWeatherForCity step, when cityName argument is Boston - return "70 and sunny" result.
            if (context.Function.Name.Equals(nameof(WeatherPlugin.GetWeatherForCity), StringComparison.OrdinalIgnoreCase) &&
                context.Arguments.TryGetValue("cityName", out object? cityName) &&
                cityName!.ToString()!.Equals("Boston", StringComparison.OrdinalIgnoreCase))
            {
                // Override step result.
                context.Result = new FunctionResult(context.Result, "70 and sunny");
            }
        }
    }

    /// <summary>
    /// Caching decorator to re-use previously generated plan and execute it.
    /// This allows to skip plan generation process for the same goal.
    /// </summary>
    private sealed class CachedChatCompletionService(IChatCompletionService innerChatCompletionService) : IChatCompletionService
    {
        /// <summary>In-memory cache for demonstration purposes.</summary>
        private readonly ConcurrentDictionary<string, string> _inMemoryCache = new();

        public IReadOnlyDictionary<string, object?> Attributes => innerChatCompletionService.Attributes;

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            // Generate cache key.
            var key = GetCacheKey(chatHistory);

            // Get chat history from cache or use original one.
            var chatHistoryToUse = this._inMemoryCache.TryGetValue(key, out string? cachedChatHistory) ?
                JsonSerializer.Deserialize<ChatHistory>(cachedChatHistory) :
                chatHistory;

            // Execute a request.
            var result = await innerChatCompletionService.GetChatMessageContentsAsync(chatHistoryToUse!, executionSettings, kernel, cancellationToken);

            // Store generated chat history in cache for future usage.
            this._inMemoryCache[key] = JsonSerializer.Serialize(chatHistoryToUse);

            return result;
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in innerChatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Hashing is used for a cache key generation for demonstration purposes.
        /// Cache key generation should be implemented based on specific scenario and requirements.
        /// </summary>
        private static string GetCacheKey(ChatHistory chatHistory)
        {
            var goal = chatHistory.First(l => l.Role == AuthorRole.User).Content!;

            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(goal));

            return Convert.ToHexString(bytes).Replace("-", "").ToUpperInvariant();
        }
    }

    #region Helper methods

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

        // Import sample plugins.
        kernel.ImportPluginFromType<TimePlugin>();
        kernel.ImportPluginFromType<WeatherPlugin>();

        return kernel;
    }

    private int GetChatHistoryTokens(ChatHistory? chatHistory)
    {
        var tokens = 0;

        if (chatHistory is null)
        {
            return tokens;
        }

        foreach (var message in chatHistory)
        {
            if (message.Metadata is not null &&
                message.Metadata.TryGetValue("Usage", out object? usage) &&
                usage is ChatTokenUsage completionsUsage &&
                completionsUsage is not null)
            {
                tokens += completionsUsage.TotalTokenCount;
            }
        }

        return tokens;
    }

    private async Task<ChatMessageContent> ExecuteWithStopwatchAsync(Func<Task<ChatMessageContent>> action)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await action();

        stopwatch.Stop();

        Console.WriteLine($@"Elapsed Time: {stopwatch.Elapsed:hh\:mm\:ss\.FFF}");

        return result;
    }

    #endregion

    #region Sample plugins

    private sealed class TimePlugin
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }

    private sealed class WeatherPlugin
    {
        [KernelFunction]
        [Description("Gets the current weather for the specified city")]
        public string GetWeatherForCity(string cityName) =>
            cityName switch
            {
                "Boston" => "61 and rainy",
                "London" => "55 and cloudy",
                "Miami" => "80 and sunny",
                "Paris" => "60 and rainy",
                "Tokyo" => "50 and sunny",
                "Sydney" => "75 and sunny",
                "Tel Aviv" => "80 and sunny",
                _ => "31 and snowing",
            };
    }

    #endregion
}
