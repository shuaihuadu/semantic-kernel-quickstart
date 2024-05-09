namespace Agents;

public class Legacy_AgentTools(ITestOutputHelper output) : BaseTest(output)
{
    private readonly List<IAgent> _agents = [];

    [Fact]
    public async Task RunCodeInterpreterToolAsync()
    {
        WriteLine("======== Using CodeInterpreter tool ========");

        AgentBuilder builder = AgentHelper.CreareAgentBuilder()
            .WithInstructions("Write only code to solve the given problem without comment.");

        try
        {
            IAgent defaultAgent = Track(await builder.BuildAsync());

            IAgent codeInterpreterAgent = Track(await builder.WithCodeInterpreter().BuildAsync());

            await ChatAsync(
                defaultAgent,
                codeInterpreterAgent,
                fileId: null,
                "What is the solution to `3x + 2 = 14`",
                "What is the fibinacci sequence until 101?");
        }
        finally
        {
            await Task.WhenAll(this._agents.Select(a => a.DeleteAsync()));
        }
    }

    [Fact(Skip = "Microsoft.SemanticKernel.HttpOperationException : Response status code does not indicate success: 400 (Bad Request).")]
    public async Task RunRetrievalToolAsync()
    {
        const bool PassFileOnRequest = false;

        WriteLine("======== Using Retrieval tool ========");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIFiles(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        OpenAIFileService fileService = kernel.GetRequiredService<OpenAIFileService>();

        OpenAIFileReference result = await fileService.UploadContentAsync(
            new BinaryContent(() => Task.FromResult(EmbeddedResource.ReadStream("travelinfo.txt")!)),
            new OpenAIFileUploadExecutionSettings("travelinfo.txt", OpenAIFilePurpose.Assistants));

        string fileId = result.Id;

        WriteLine($"! {fileId}");

        IAgent defaultAgent = Track(await AgentHelper.CreareAgentBuilder().BuildAsync());

        IAgent retrievalAgent = Track(await AgentHelper.CreareAgentBuilder().BuildAsync());

        if (!PassFileOnRequest)
        {
            await retrievalAgent.AddFileAsync(fileId);
        }

        try
        {
            await ChatAsync(
                defaultAgent,
                retrievalAgent,
                PassFileOnRequest ? fileId : null,
                "Where did sam go?",
                "When does the flight leave Seattle?",
                "What is the hotel contact info at the destination>");
        }
        finally
        {
            await Task.WhenAll(this._agents.Select(a => a.DeleteAsync()));
        }
    }

    private async Task ChatAsync(
        IAgent defaultAgent,
        IAgent enableAgent,
        string? fileId = null,
        params string[] questions)
    {
        string[]? fileIds = null;

        if (fileId != null)
        {
            fileIds = [fileId];
        }

        foreach (string question in questions)
        {
            WriteLine("\nDEFAULT AGENT:");
            await InvokeAgentAsync(defaultAgent, question, fileIds);

            WriteLine("\nTOOL ENABLED AGENT:");
            await InvokeAgentAsync(enableAgent, question, fileIds);
        }
    }

    private async Task InvokeAgentAsync(IAgent agent, string question, string[]? fileIds)
    {
        await foreach (IChatMessage message in agent.InvokeAsync(question, null, fileIds))
        {
            string content = message.Content;

            foreach (var annotation in message.Annotations)
            {
                content = content.Replace(annotation.Label, string.Empty, StringComparison.Ordinal);
            }

            WriteLine($"# {message.Role}: {content}");

            if (message.Annotations.Count > 0)
            {
                WriteLine("\n# files:");

                foreach (var annotation in message.Annotations)
                {
                    WriteLine($"* {annotation.FileId}");
                }
            }
        }

        WriteLine();
    }

    private IAgent Track(IAgent agent)
    {
        this._agents.Add(agent);
        return agent;
    }
}
