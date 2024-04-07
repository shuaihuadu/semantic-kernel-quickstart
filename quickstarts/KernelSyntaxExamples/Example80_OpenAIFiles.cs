
namespace KernelSyntaxExamples;

public sealed class Example80_OpenAIFiles(ITestOutputHelper output) : BaseTest(output)
{
    private const string ResourceFileName = "30-user-context.txt";

    [Fact(Skip = "TODO Azure OpenAI File")]
    public void RunFileLifecycleAsync()
    {
        this.WriteLine("======== OpenAI File-Service ========");
    }
}
