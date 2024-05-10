using Microsoft.SemanticKernel.Experimental.Agents;

namespace Agents;

public class Legacy_Agents(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public Task RunSimpleChatAsync()
    {
        Console.WriteLine("======== Run:SimpleChat ========");

        return ChatAsync("Agents.ParrotAgent.yaml",
            plugin: null,
            arguments: new KernelArguments() { { "count", 3 } },
            "Fortune favors the bold.",
            "I came, I saw, I conquered.",
            "Practice makes perfect.");
    }

    [Fact]
    public Task RunWithMethodFunctionsAsync()
    {
        Console.WriteLine("======== Run:WithMethodFunctions ========");

        LegacyMenuPlugin menuApi = new();

        KernelPlugin plugin = KernelPluginFactory.CreateFromObject(menuApi);

        return ChatAsync(
            "Agents.ParrotAgent.yaml",
            plugin,
            arguments: new() { { LegacyMenuPlugin.CorrelationIdArgument, 3.141592653 } },
            "Hello",
            "What is the special soup?",
            "What is the special drink?",
            "Do you have enough soup for 5 orders?",
            "Thank you!");
    }

    [Fact(Skip = "Runtime type 'Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIChatMessageContent' is not supported by polymorphic type 'Microsoft.SemanticKernel.KernelContent'. Path: $.")]
    public Task RunWithPromptFunctionsAsync()
    {
        Console.WriteLine("======== WithPromptFunctions ========");

        KernelFunction function = KernelFunctionFactory.CreateFromPrompt(
             "Correct any misspelling or gramatical errors provided in input: {{$input}}",
            functionName: "SpellChecker",
            description: "Correct the spelling for the user input.");

        KernelPlugin plugin = KernelPluginFactory.CreateFromFunctions("spelling", "Spelling functions", [function]);

        return ChatAsync(
            "Agents.ToolAgent.yaml",
            plugin,
            arguments: null,
            "Hello",
            "Is this spelled correctly: exercize",
            "What is the special soup?",
            "Thank you!");
    }

    [Fact]
    public async Task RunAsFunctionAsync()
    {
        Console.WriteLine("======== Run:AsFunction ========");

        IAgent agent = await new AgentBuilder()
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.DeploymentName, TestConfiguration.AzureOpenAI.ApiKey)
            .FromTemplate(EmbeddedResource.Read("Agents.ParrotAgent.yaml"))
            .BuildAsync();

        try
        {
            string response = await agent.AsPlugin().InvokeAsync("Practice makes perfect.", new KernelArguments { { "count", 2 } });

            Console.WriteLine(response ?? $"No response from agent: {agent.Id}");
        }
        finally
        {
            await agent.DeleteAsync();
        }
    }

    private async Task ChatAsync(string resourcePath, KernelPlugin? plugin = null, KernelArguments? arguments = null, params string[] messages)
    {
        string definition = EmbeddedResource.Read(resourcePath);

        IAgent agent = await new AgentBuilder()
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.DeploymentName, TestConfiguration.AzureOpenAI.ApiKey)
            .FromTemplate(definition)
            .WithPlugin(plugin)
            .BuildAsync();

        IAgentThread thread = await agent.NewThreadAsync();

        try
        {
            Console.WriteLine($"[{agent.Id}]");

            foreach (var responses in messages.Select(m => thread.InvokeAsync(agent, m, arguments)))
            {
                await foreach (var message in responses)
                {
                    Console.WriteLine($"[{message.Id}]");
                    Console.WriteLine($"# {message.Role}: {message.Content}");
                }
            }
        }
        finally
        {
            await Task.WhenAll(thread?.DeleteAsync() ?? Task.CompletedTask, agent.DeleteAsync());
        }
    }
}
