namespace KernelSyntaxExamples;

public class Example73_AgentAuthoring : BaseTest
{
    [Fact(Skip = "When Agent Support Azure OpenAI")]
    public Task RunAsync()
    {
        return Task.CompletedTask;
    }

    public Example73_AgentAuthoring(ITestOutputHelper output) : base(output)
    {
    }
}
