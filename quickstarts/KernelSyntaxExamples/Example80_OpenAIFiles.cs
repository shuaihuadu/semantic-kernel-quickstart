
namespace KernelSyntaxExamples;

public sealed class Example80_OpenAIFiles : BaseTest
{
    private const string ResourceFileName = "30-user-context.txt";

    [Fact(Skip = "TODO Azure OpenAI File")]
    public async Task RunFileLifecycleAsync()
    {
        this.WriteLine("======== OpenAI File-Service ========");
    }

    public Example80_OpenAIFiles(ITestOutputHelper output) : base(output)
    {
    }
}
