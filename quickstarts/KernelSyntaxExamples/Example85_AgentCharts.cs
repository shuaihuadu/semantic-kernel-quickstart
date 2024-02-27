namespace KernelSyntaxExamples;

public class Example85_AgentCharts(ITestOutputHelper output) : BaseTest(output)
{
    [Fact(Skip = "Azure OpenAI Function Call")]
    public async Task CreateChartAsync()
    {
        this.WriteLine("======== Using CodeInterpreter tool ========");

        this.WriteLine(Environment.CurrentDirectory);

        OpenAIFileService fileService = new(TestConfiguration.OpenAI.ApiKey);

        IAgent agent = await new AgentBuilder()
            .WithAzureOpenAIChatCompletion(
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                model: TestConfiguration.AzureOpenAI.ChatModelId,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .WithCodeInterpreter()
            .BuildAsync();

        try
        {
            IAgentThread thread = await agent.NewThreadAsync();

            await InvokeAgentAsync(thread, "1-first", @"
Display this data using a bar-chart:

Banding  Brown Pink Yellow  Sum
X00000   339   433     126  898
X00300    48   421     222  691
X12345    16   395     352  763
Others    23   373     156  552
Sum      426  1622     856 2904
");

            await InvokeAgentAsync(thread, "2-color", "Can you regenerate this same chart using the category names as the bar colors?");

            await InvokeAgentAsync(thread, "3-line", "Can you regenerate this as a line chart?");
        }
        finally
        {
            await agent.DeleteAsync();
        }

        async Task InvokeAgentAsync(IAgentThread thread, string imageName, string question)
        {
            await foreach (IChatMessage? message in thread.InvokeAsync(agent, question))
            {
                if (message.ContentType == ChatMessageType.Image)
                {
                    string fileName = $"{imageName}.jpg";

                    BinaryContent content = fileService.GetFileContent(fileName);

                    await using FileStream outputStream = File.OpenWrite(fileName);
                    await using Stream inputStream = await content.GetStreamAsync();
                    await inputStream.CopyToAsync(outputStream);

                    string path = Path.Join(Environment.CurrentDirectory, fileName);

                    this.WriteLine($"# {message.Role}: {path}");

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C start {path}"
                    });
                }
                else
                {
                    this.WriteLine($"# {message.Role}: {message.Content}");
                }
            }
        }
    }
}
