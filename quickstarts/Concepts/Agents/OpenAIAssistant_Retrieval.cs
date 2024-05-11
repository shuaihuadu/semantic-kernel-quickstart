namespace Agents;

public class OpenAIAssistant_Retrieval(ITestOutputHelper output) : BaseTest(output)
{
    [Fact(Skip = "Azure.RequestFailedException : Retrieval tools are not supported.")]
    public async Task RunAsync()
    {
        OpenAIFileService fileService = new OpenAIFileService(new Uri(TestConfiguration.AzureOpenAI.Endpoint), TestConfiguration.AzureOpenAI.ApiKey);

        OpenAIFileReference uploadFile = await fileService.UploadContentAsync(
            new BinaryContent(() => Task.FromResult(EmbeddedResource.ReadStream("travelinfo.txt")!)),
            new OpenAIFileUploadExecutionSettings("travelinfo.txt", OpenAIFilePurpose.Assistants));

        OpenAIAssistantAgent agent = await OpenAIAssistantAgent.CreateAsync(
            kernel: new(),
            config: new(TestConfiguration.AzureOpenAI.ApiKey, TestConfiguration.AzureOpenAI.Endpoint),
            new()
            {
                EnableRetrieval = true,
                ModelId = TestConfiguration.AzureOpenAI.DeploymentName,
                FileIds = [uploadFile.Id]
            });

        AgentGroupChat chat = new();

        try
        {
            await InvokeAgentAsync("Where did sam go?");
            await InvokeAgentAsync("When does the flight leave Seattle?");
            await InvokeAgentAsync("What is the hotel contact info at the destination?");
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
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}:'{content.Content}'");
            }
        }
    }
}
