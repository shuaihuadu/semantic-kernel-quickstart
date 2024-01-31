namespace KernelSyntaxExamples;

public class Example72_AgentCollaboration : BaseTest
{
    [Fact(Skip = "When Agent Support Azure OpenAI")]
    public Task RunAsync()
    {
        return Task.CompletedTask;
    }

    public Example72_AgentCollaboration(ITestOutputHelper output) : base(output)
    {
    }
}
