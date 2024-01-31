namespace KernelSyntaxExamples;

public class Example70_Agents : BaseTest
{
    [Fact(Skip = "When Agent Support Azure OpenAI")]
    public Task RunAsync()
    {
        return Task.CompletedTask;
    }

    public Example70_Agents(ITestOutputHelper output) : base(output)
    {
    }
}
