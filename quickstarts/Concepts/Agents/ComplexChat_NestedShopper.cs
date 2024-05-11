using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Agents.Chat;

namespace Agents;

public class ComplexChat_NestedShopper(ITestOutputHelper output) : BaseTest(output)
{
    private const string InternalLeaderName = "InternalLeader";
    private const string InternalLeaderInstructions = """
        您的工作是向用户清楚、直接地传达当前助理的响应。
        如果请求了信息，只需重复请求。
        如果提供了信息，只需重复该信息。
        不要提出自己的购物建议。
        """;

    private const string InternalGiftIdeaAgentName = "InternalGiftIdeas";
    private const string InternalGiftIdeaAgentInstructions = """
        你是一个提供礼品创意的个人购物者。
        只有在了解礼物接受者的以下情况时，才能提供想法：
        - 与送礼者的关系
        - 赠送原因
        在提供想法之前，请索取任何缺失的信息。
        只描述礼物的名字。
        始终立即纳入审查反馈并提供最新回复。
        """;

    private const string InternalGiftReviewerName = "InternalGiftReviewer";
    private const string InternalGiftReviewerInstructions =
        """
        查看最近的购物回复。
        要么在不引入新想法的情况下提供关键反馈以改进响应，要么声明响应是充分的。
        """;

    private const string InnerSelectionInstructions =
        $$$"""
        根据对话历史记录选择下一轮的参与者。

        只能从以下参与者中选择：
        - {{{InternalGiftIdeaAgentName}}}
        - {{{InternalGiftReviewerName}}}
        - {{{InternalLeaderName}}}

        根据最近参与者的操作选择下一个参与者：
        - 用户输入后，轮到 {{{InternalGiftIdeaAgentName}}} 了。
        - 在 {{{InternalGiftIdeaAgentName}}} 回复想法后，轮到 {{{InternalGiftReviewerName}}} 了。
        - 在 {{{InternalGiftIdeaAgentName}}} 请求其他信息后，轮到 {{{InternalLeaderName}}} 了。
        - 在 {{{InternalGiftReviewerName}}} 提供反馈或指示后，该轮到 {{{InternalGiftIdeaAgentName}}} 了。
        - 在 {{{InternalGiftReviewerName}}} 声明 {{{InternalGiftIdeaAgentName}}}的响应足够后，轮到{{{InternalLeaderName}}} 了。

        以JSON格式响应，JSON的结构只能包括：
        {
            "name": "string(为下一轮选择的助手的名称)",
            "reason": "string(选择参与者的原因)"
        }

        历史记录:
        {{${{{KernelFunctionSelectionStrategy.DefaultHistoryVariableName}}}}}
        """;

    private const string OuterTerminationInstructions =
        $$$"""
        确定用户请求是否已得到完全响应。
        
        以JSON格式响应，JSON的结构只能包括：
        {
            "isAnswered": "bool (如果用户请求已得到完全响应，则为true)",
            "reason": "string (你下定决心的原因)"
        }
        
        历史记录:
        {{${{{KernelFunctionTerminationStrategy.DefaultHistoryVariableName}}}}}
        """;

    [Fact]
    public async Task RunAsync()
    {
        Console.WriteLine($"! {Model}");

        OpenAIPromptExecutionSettings jsonSettings = new() { ResponseFormat = ChatCompletionsResponseFormat.JsonObject };
        OpenAIPromptExecutionSettings autoInvokeSettings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        ChatCompletionAgent internalLeaderAgent = CreateAgent(InternalLeaderName, InternalLeaderInstructions);
        ChatCompletionAgent internalGiftIdeaAgent = CreateAgent(InternalGiftIdeaAgentName, InternalGiftIdeaAgentInstructions);
        ChatCompletionAgent internalGiftReviewerAgent = CreateAgent(InternalGiftReviewerName, InternalGiftReviewerInstructions);

        KernelFunction innerSelectionFunction = KernelFunctionFactory.CreateFromPrompt(InnerSelectionInstructions, jsonSettings);
        KernelFunction outerTerminationFunction = KernelFunctionFactory.CreateFromPrompt(OuterTerminationInstructions, jsonSettings);

        AggregatorAgent personalShopperAgent = new(CreateChat)
        {
            Name = "PersonalShopper",
            Mode = AggregatorMode.Nested
        };

        AgentGroupChat chat = new(personalShopperAgent)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new KernelFunctionTerminationStrategy(outerTerminationFunction, CreateKernelWithChatCompletion())
                {
                    ResultParser = (result) =>
                    {
                        OuterTerminationResult? jsonResult = JsonResultTranslator.Translate<OuterTerminationResult>(result.GetValue<string>());

                        return jsonResult?.isAnswered ?? false;
                    },
                    MaximumIterations = 5
                }
            }
        };

        Console.WriteLine("\n######################################");
        Console.WriteLine("# DYNAMIC CHAT");
        Console.WriteLine("######################################");

        await InvokeChatAsync("你能提供三个原创的生日礼物创意吗。我不想要别人也会挑选的礼物。");

        await InvokeChatAsync("这份礼物是给我成年的弟弟的。");

        if (!chat.IsComplete)
        {
            await InvokeChatAsync("他喜欢摄影");
        }
        Console.WriteLine("\n\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        Console.WriteLine(">>>> AGGREGATED CHAT");
        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");

        await foreach (var content in chat.GetChatMessagesAsync(personalShopperAgent).Reverse())
        {
            Console.WriteLine($">>>> {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
        }

        async Task InvokeChatAsync(string input)
        {
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

            Console.WriteLine($"# {AuthorRole.User}: '{input}'");

            await foreach (var content in chat.InvokeAsync(personalShopperAgent))
            {
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
            }

            Console.WriteLine($"\n# IS COMPLETE: {chat.IsComplete}");
        }

        ChatCompletionAgent CreateAgent(string agentName, string agentInstructions)
        {
            return new ChatCompletionAgent
            {
                Instructions = agentInstructions,
                Name = agentName,
                Kernel = this.CreateKernelWithChatCompletion()
            };
        }

        AgentGroupChat CreateChat() => new(internalLeaderAgent, internalGiftReviewerAgent, internalGiftIdeaAgent)
        {
            ExecutionSettings = new()
            {
                SelectionStrategy = new KernelFunctionSelectionStrategy(innerSelectionFunction, CreateKernelWithChatCompletion())
                {
                    ResultParser = (result) =>
                    {
                        AgentSelectionResult? jsonResult = JsonResultTranslator.Translate<AgentSelectionResult>(result.GetValue<string>());

                        string? agentName = string.IsNullOrWhiteSpace(jsonResult?.name) ? null : jsonResult?.name;

                        agentName ??= InternalGiftIdeaAgentName;

                        Console.WriteLine($"\t>>>> INNER TURN: {agentName}");

                        return agentName;
                    }
                },
                TerminationStrategy = new AgentTerminationStrategy()
                {
                    Agents = [internalLeaderAgent],
                    MaximumIterations = 7,
                    AutomaticReset = true
                }
            }
        };
    }

    private sealed record OuterTerminationResult(bool isAnswered, string reason);

    private sealed record AgentSelectionResult(string name, string reason);

    private sealed class AgentTerminationStrategy : TerminationStrategy
    {
        /// <inheritdoc />
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
