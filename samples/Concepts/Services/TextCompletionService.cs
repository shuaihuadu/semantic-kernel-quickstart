using Microsoft.SemanticKernel.ChatCompletion;

namespace MCS.Library.AI.AzureOpenAI;

public class TextCompletionService
{
    private readonly ILogger<TextCompletionService> _logger;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    private const string DefaultSkillName = "Default";

    public TextCompletionService(ILoggerFactory loggerFactory, Kernel kernel)
    {
        this._logger = loggerFactory.CreateLogger<TextCompletionService>();
        this._kernel = kernel;
        this._chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        string pluginDirectory = Path.Join(AppContext.BaseDirectory, "Services", "Skills", DefaultSkillName);
        kernel.ImportPluginFromPromptDirectory(pluginDirectory);
    }


    public async Task<string> TalkAsync(ChatHistory chatHistory)
    {
        ChatMessageContent chatMessageContent = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);

        return chatMessageContent.Content ?? string.Empty;
    }

    public async Task<string> ChatAsync(string skill, string input, string? question = null, string? chatHistory = null)
    {
        return await ExecuteWithSkillAsync(skill, input, question, chatHistory);
    }

    public async Task<string> ChatAsync(string functionName, string prompt, PromptExecutionSettings executionSettings, KernelArguments? arguments = null, CancellationTokenSource? cancellationTokenSource = null)
    {
        return await InnerChatAsync(prompt, executionSettings, arguments, functionName, cancellationTokenSource);
    }

    private async Task<string> InnerChatAsync(string prompt, PromptExecutionSettings executionSettings, KernelArguments? arguments = null, string? functionName = null, CancellationTokenSource? cancellationTokenSource = null)
    {
        cancellationTokenSource ??= new();
        arguments ??= [];

        KernelFunction function = _kernel.CreateFunctionFromPrompt(prompt, executionSettings, functionName);

        FunctionResult functionResult = await _kernel.InvokeAsync(function, arguments, cancellationTokenSource.Token);

        return functionResult.ToString();
    }

    /// <summary>
    /// 使用指定的Skill执行Semantic Search
    /// </summary>
    /// <param name="skill">使用的Skill</param>
    /// <param name="question">问题内容</param>
    /// <param name="input">问题输入的上下文</param>
    /// <param name="chatHistory">问题的历史记录</param>
    /// <param name="cancellationTokenSource"></param>
    /// <returns></returns>
    private async Task<string> ExecuteWithSkillAsync(string skill, string input, string? question = null, string? chatHistory = null, CancellationTokenSource? cancellationTokenSource = null)
    {
        cancellationTokenSource ??= new();

        KernelArguments arguments = new()
        {
            ["input"] = input
        };

        if (question is not null)
        {
            arguments["question"] = question;
        }

        if (chatHistory is not null)
        {
            arguments["chatHistory"] = chatHistory;
        }

        FunctionResult functionResult = await _kernel.InvokeAsync(pluginName: DefaultSkillName, functionName: skill, arguments, cancellationToken: cancellationTokenSource.Token);

        var answer = functionResult.ToString();

        _logger.LogInformation("ExecuteWithSkillAsync use skill:{Skill} with input:{Input}, and question:{Question}, and chatHistory as bellow:{ChatHistory} the GPT original answer as bellow:{Answer}", skill, input, question, chatHistory, answer);

        return answer ?? string.Empty;
    }
}
