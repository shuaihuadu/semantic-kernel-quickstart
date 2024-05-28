using Microsoft.SemanticKernel.Agents.Chat;
using ChatCompletionAgent = Microsoft.SemanticKernel.Agents.ChatCompletionAgent;

namespace Agents;

public class MixedChat_Agents(ITestOutputHelper output) : BaseTest(output)
{
    private const string ReviewerName = "ArtDirector";
    private const string ReviewerInstrunctions = """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine is the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without example.
        """;
    private const string CopyWriterName = "CopyWriter";
    private const string CopyWriterInstructions =
        """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """;


    [Fact]
    public async Task RunAsync()
    {
        ChatCompletionAgent agentReviewer = new()
        {
            Instructions = ReviewerInstrunctions,
            Name = ReviewerName,
            Kernel = KernelHelper.CreateKernelWithAzureOpenAIChatCompletion()
        };

        OpenAIAssistantAgent agentWriter = await OpenAIAssistantAgent.CreateAsync(kernel: new(), config: new OpenAIAssistantConfiguration(TestConfiguration.AzureOpenAI.ApiKey, TestConfiguration.AzureOpenAI.Endpoint), definition: new()
        {
            Instructions = CopyWriterInstructions,
            Name = CopyWriterName,
            ModelId = TestConfiguration.AzureOpenAI.DeploymentName
        });

        AgentGroupChat chat = new(agentWriter, agentReviewer)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new ApprovalTerminationStrategy()
                {
                    Agents = [agentReviewer],
                    MaximumIterations = 10
                }
            }
        };

        string input = "concept: maps made out of egg cartons.";

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

        Console.WriteLine($"# {AuthorRole.User}: '{input}'");

        await foreach (var content in chat.InvokeAsync())
        {
            Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
        }

        Console.WriteLine($"# IS COMPLETE: {chat.IsComplete}");
    }

    private sealed class ApprovalTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        {
            return Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}
