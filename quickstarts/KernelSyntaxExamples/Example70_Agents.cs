namespace KernelSyntaxExamples;

public class Example70_Agents(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public Task RunAsync()
    {
        return Task.CompletedTask;
    }

    //[Fact]
    //public Task RunWithMethodFunctionsAsync()
    //{
    //    WriteLine("======== Run:WithMethodFunctions ========");

    //    KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
    //}

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
