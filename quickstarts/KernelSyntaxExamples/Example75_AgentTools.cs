namespace KernelSyntaxExamples;

public class Example75_AgentTools : BaseTest
{
    [Fact(Skip = "When Agent Support Azure OpenAI")]
    public Task RunAsync()
    {
        return Task.CompletedTask;
    }

    public Example75_AgentTools(ITestOutputHelper output) : base(output)
    {
    }
}
