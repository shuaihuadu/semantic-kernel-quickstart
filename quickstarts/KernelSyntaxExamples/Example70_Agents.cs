namespace KernelSyntaxExamples;

public class Example70_Agents(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public Task RunSimpleChatAsync()
    {
        WriteLine("======== Run:SimpleChat ========");

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
        WriteLine("======== Run:WithMethodFunctions ========");

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();

        return ChatAsync(
            "Agents.ParrotAgent.yaml",
            plugin,
            arguments: null,
            "Hello",
            "What is the special soup?",
            "What is the special drink?",
            "Thank you!");
    }

    [Fact(Skip = "Microsoft.SemanticKernel.HttpOperationException : Incorrect API key provided: You can find your API key at https://platform.openai.com/account/api-keys.")]
    public Task RunWithPromptFunctionsAsync()
    {
        WriteLine("======== WithPromptFunctions ========");

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
        WriteLine("======== Run:AsFunction ========");

        IAgent agent = await new AgentBuilder()
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ChatDeploymentName, TestConfiguration.AzureOpenAI.ApiKey)
            .FromTemplate(EmbeddedResource.Read("Agents.ParrotAgent.yaml"))
            .BuildAsync();

        try
        {
            string response = await agent.AsPlugin().InvokeAsync("Practice makes perfect.", new KernelArguments { { "count", 2 } });

            WriteLine(response ?? $"No response from agent: {agent.Id}");
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
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ChatModelId, TestConfiguration.AzureOpenAI.ApiKey)
            .FromTemplate(definition)
            .WithPlugin(plugin)
            .BuildAsync();

        IAgentThread thread = await agent.NewThreadAsync();

        try
        {
            WriteLine($"[{agent.Id}]");

            foreach (var responses in messages.Select(m => thread.InvokeAsync(agent, m, arguments)))
            {
                await foreach (var message in responses)
                {
                    WriteLine($"[{message.Id}]");
                    WriteLine($"# {message.Role}: {message.Content}");
                }
            }
        }
        finally
        {
            await Task.WhenAll(thread?.DeleteAsync() ?? Task.CompletedTask, agent.DeleteAsync());
        }
    }
}
