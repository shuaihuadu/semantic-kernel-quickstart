using Microsoft.SemanticKernel.Experimental.Agents;

namespace Agents;

public class Legacy_AgentDelegation(ITestOutputHelper output) : BaseTest(output)
{
    private static readonly List<IAgent> agents = [];

    [Fact]
    public async Task RunAsync()
    {
        Console.WriteLine("======== Example71_AgentDelegation ========");

        IAgentThread? threand = null;

        try
        {
            KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();

            IAgent menuAgent = Track(await AgentHelper.CreareAgentBuilder()
                .FromTemplate(EmbeddedResource.Read("Agents.ToolAgent.yaml"))
                .WithDescription("Answer questions about how the menu uses the tool.")
                .WithPlugin(plugin)
                .BuildAsync());

            IAgent parrotAgent = Track(await AgentHelper.CreareAgentBuilder()
                .FromTemplate(EmbeddedResource.Read("Agents.ParrotAgent.yaml"))
                .BuildAsync());

            IAgent toolAgent = Track(await AgentHelper.CreareAgentBuilder()
                .FromTemplate(EmbeddedResource.Read("Agents.ToolAgent.yaml"))
                .WithPlugin(parrotAgent.AsPlugin())
                .WithPlugin(menuAgent.AsPlugin())
                .BuildAsync());

            string[] messages =
            [
                "What's on the menu?",
                "Can you talk like pirate?",
                "Thank you"
            ];

            threand = await toolAgent.NewThreadAsync();

            foreach (var response in messages.Select(m => threand.InvokeAsync(toolAgent, m)))
            {
                await foreach (IChatMessage message in response)
                {
                    Console.WriteLine($"[{message.Id}]");
                    Console.WriteLine($"# {message.Role}: {message.Content}");
                }
            }
        }
        finally
        {
            await Task.WhenAll(
                threand?.DeleteAsync() ?? Task.CompletedTask,
                Task.WhenAll(agents.Select(a => a.DeleteAsync())));
        }
    }

    private static IAgent Track(IAgent agent)
    {
        agents.Add(agent);

        return agent;
    }
}
