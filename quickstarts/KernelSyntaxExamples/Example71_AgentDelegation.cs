namespace KernelSyntaxExamples;

public class Example71_AgentDelegation : BaseTest
{
    [Fact(Skip = "When Agent Support Azure OpenAI")]
    public Task RunAsync()
    {
        return Task.CompletedTask;
    }

    public Example71_AgentDelegation(ITestOutputHelper output) : base(output)
    {
    }
}
