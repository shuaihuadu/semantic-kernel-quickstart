﻿namespace Agents;

public class OpenAIAssistant_ChartMaker(ITestOutputHelper output) : BaseTest(output)
{
    private const string AgentName = "ChartMaker";
    private const string AgentInstructions = "Create charts as requested without explanation.";

    [Fact]
    public async Task RunAsync()
    {
        OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(kernel: new(),
            config: new OpenAIAssistantConfiguration(TestConfiguration.AzureOpenAI.ApiKey, TestConfiguration.AzureOpenAI.Endpoint),
            new()
            {
                Instructions = AgentInstructions,
                Name = AgentName,
                EnableCodeInterpreter = true,
                ModelId = TestConfiguration.AzureOpenAI.DeploymentName
            });

        AgentGroupChat chat = new();

        try
        {
            await InvokeAgentAsync(
                """
                Display this data using a bar-chart:

                Banding  Brown Pink Yellow  Sum
                X00000   339   433     126  898
                X00300    48   421     222  691
                X12345    16   395     352  763
                Others    23   373     156  552
                Sum      426  1622     856 2904
                """);

            await InvokeAgentAsync("Can you regenerate this same chart using the category names as the bar colors?");
        }
        finally
        {
            await agent.DeleteAsync();
        }

        async Task InvokeAgentAsync(string input)
        {
            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

            Console.WriteLine($"# {AuthorRole.User}: '{input}'");

            await foreach (var message in chat.InvokeAsync(agent))
            {
                if (!string.IsNullOrWhiteSpace(message.Content))
                {
                    Console.WriteLine($"# {message.Role} - {message.AuthorName ?? "*"}:'{message.Content}'");
                }

                foreach (var fileReference in message.Items.OfType<FileReferenceContent>())
                {
                    Console.WriteLine($"# {message.Role} - {message.AuthorName ?? "*"}:#{fileReference.FileId}");
                }
            }
        }
    }
}
