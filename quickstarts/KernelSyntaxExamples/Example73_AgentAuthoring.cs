namespace KernelSyntaxExamples;

public class Example73_AgentAuthoring(ITestOutputHelper output) : BaseTest(output)
{
    [Fact(Skip = "When Agent Support Azure OpenAI")]
    public Task RunAsync()
    {
        return Task.CompletedTask;
    }
}
