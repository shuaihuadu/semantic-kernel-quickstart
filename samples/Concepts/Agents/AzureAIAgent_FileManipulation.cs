using Azure.AI.Projects;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Resources;
using Agent = Azure.AI.Projects.Agent;


namespace Agents;

[TestClass]
public class AzureAIAgent_FileManipulation : BaseAzureAgentTest
{
    [TestMethod]
    public async Task AnalyzeCSVFileUsingAzureAIAgentAsync()
    {
        await using Stream stream = EmbeddedResource.ReadStream("sales.csv")!;

        AgentFile fileInfo = await this.AgentsClient.UploadFileAsync(stream, AgentFilePurpose.Agents, "sales.csv");

        Agent definition = await this.AgentsClient.CreateAgentAsync(
            TestConfiguration.AzureAI.ChatModelId,
            tools: [new CodeInterpreterToolDefinition()],
            toolResources:
                new()
                {
                    CodeInterpreter = new()
                    {
                        FileIds = { fileInfo.Id },
                    }
                });
        AzureAIAgent agent = new(definition, this.AgentsClient);

        AgentGroupChat chat = new();

        try
        {
            await InvokeAgentAsync("Which segment had the most sales?");
            await InvokeAgentAsync("List the top 5 countries that generated the most profit.");
            await InvokeAgentAsync("Create a tab delimited file report of profit by each country per month.");
        }
        finally
        {
            await this.AgentsClient.DeleteAgentAsync(agent.Id);
            await this.AgentsClient.DeleteFileAsync(fileInfo.Id);
            await chat.ResetAsync();
        }

        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.AddChatMessage(new(AuthorRole.User, input));
            this.WriteAgentChatMessage(message);

            await foreach (ChatMessageContent response in chat.InvokeAsync(agent))
            {
                this.WriteAgentChatMessage(response);
                await this.DownloadContentAsync(response);
            }
        }
    }
}
