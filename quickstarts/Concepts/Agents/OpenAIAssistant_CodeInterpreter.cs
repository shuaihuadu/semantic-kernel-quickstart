namespace Agents;

public class OpenAIAssistant_CodeInterpreter(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(kernel: new(),
            config: new(TestConfiguration.AzureOpenAI.ApiKey, TestConfiguration.AzureOpenAI.Endpoint),
            new()
            {
                EnableCodeInterpreter = true,
                ModelId = TestConfiguration.AzureOpenAI.DeploymentName
            });

        AgentGroupChat chat = new();

        try
        {
            await InvokeAgentAsync("What is the solution to `3x + 2 = 14`?");
            await InvokeAgentAsync("What is the fibinacci sequence until 101?");
        }
        finally
        {
            await agent.DeleteAsync();
        }

        async Task InvokeAgentAsync(string input)
        {
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

            Console.WriteLine($"# {AuthorRole.User}: '{input}'");

            await foreach (var content in chat.InvokeAsync(agent))
            {
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}:{content.Content}");
            }
        }
    }
}
