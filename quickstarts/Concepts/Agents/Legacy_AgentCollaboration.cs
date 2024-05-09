namespace Agents;

public class Legacy_AgentCollaboration(ITestOutputHelper output) : BaseTest(output)
{
    private static readonly List<IAgent> agents = [];

    [Fact]
    public async Task RunCollaborationAsync()
    {
        WriteLine($"======== Example72:Collaboration: AzureAI ========");

        IAgentThread? thread = null;

        try
        {
            IAgent copyWriter = await CreateCopyWriterAsync();
            IAgent artDirector = await CreateArtDirectorAsync();

            thread = await copyWriter.NewThreadAsync();

            IChatMessage messageUser = await thread.AddUserMessageAsync("concept: maps made out of egg cartons.");

            DisplayMessage(messageUser);

            bool isCompleted = false;

            do
            {
                IChatMessage[] agentMessages = await thread.InvokeAsync(copyWriter).ToArrayAsync();
                DisplayMessage(agentMessages);

                agentMessages = await thread.InvokeAsync(artDirector).ToArrayAsync();
                DisplayMessage(agentMessages);

                if (agentMessages.First().Content.Contains("PRINT IT", StringComparison.OrdinalIgnoreCase))
                {
                    isCompleted = true;
                }
            }
            while (!isCompleted);
        }
        finally
        {
            await Task.WhenAll(agents.Select(a => a.DeleteAsync()));
        }

    }

    [Fact]
    public async Task RunAsPluginsAsync()
    {
        WriteLine($"======== Example72: AsPlugins: AzureAI ========");

        try
        {
            IAgent copyWriter = await CreateCopyWriterAsync();

            IAgent artDirector = await CreateArtDirectorAsync();

            IAgent coordinator = Track(
                await CreateAgentBuilder()
                .WithInstructions("Reply the provided concept and have the copy-writer generate an marketing idea (copy).  Then have the art-director reply to the copy-writer with a review of the copy.  Always include the source copy in any message.  Always include the art-director comments when interacting with the copy-writer.  Coordinate the repeated replies between the copy-writer and art-director until the art-director approves the copy.")
                .WithPlugin(copyWriter.AsPlugin())
                .WithPlugin(artDirector.AsPlugin())
                .BuildAsync());

            string response = await coordinator.AsPlugin().InvokeAsync("concept: maps made out of egg cartons.");

            WriteLine(response);
        }
        finally
        {
            await Task.WhenAll(agents.Select(a => a.DeleteAsync()));
        }
    }

    private async static Task<IAgent> CreateCopyWriterAsync(IAgent? agent = null)
    {
        return
            Track(
                await CreateAgentBuilder()
                    .WithInstructions("You are a copywriter with ten years of experience and are known for brevity and a dry humor. You're laser focused on the goal at hand. Don't waste time with chit chat. The goal is to refine and decide on the single best copy as an expert in the field.  Consider suggestions when refining an idea.")
                    .WithName("Copywriter")
                    .WithDescription("Copywriter")
                    .WithPlugin(agent?.AsPlugin())
                    .BuildAsync());
    }

    private async static Task<IAgent> CreateArtDirectorAsync()
    {
        return
            Track(
                await CreateAgentBuilder()
                    .WithInstructions("You are an art director who has opinions about copywriting born of a love for David Ogilvy. The goal is to determine is the given copy is acceptable to print, even if it isn't perfect.  If not, provide insight on how to refine suggested copy without example.  Always respond to the most recent message by evaluating and providing critique without example.  Always repeat the copy at the beginning.  If copy is acceptable and meets your criteria, say: PRINT IT.")
                    .WithName("Art Director")
                    .WithDescription("Art Director")
                    .BuildAsync());
    }

    private static AgentBuilder CreateAgentBuilder()
    {
        AgentBuilder builder = new();

        return builder.WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.DeploymentName, TestConfiguration.AzureOpenAI.ApiKey);
    }

    private void DisplayMessage(IEnumerable<IChatMessage> messages, IAgent? agent = null)
    {
        foreach (var message in messages)
        {
            DisplayMessage(message, agent);
        }
    }

    private void DisplayMessage(IChatMessage message, IAgent? agent = null)
    {
        WriteLine($"[{message.Id}]");

        if (agent is not null)
        {
            WriteLine($"# {message.Role}: ({agent.Name}) {message.Content}");
        }
        else
        {
            WriteLine($"# {message.Role}: {message.Content}");
        }
    }

    private static IAgent Track(IAgent agent)
    {
        agents.Add(agent);

        return agent;
    }
}
