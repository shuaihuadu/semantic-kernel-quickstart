
namespace KernelSyntaxExamples;

public class Example01_MethodFunctions : BaseTest
{
    [Fact]
    public Task RunAsync()
    {
        this.WriteLine("======== Functions ========");

        TextPlugin textPlugin = new();

        string result = textPlugin.Uppercase("quick start");

        this.WriteLine(result);

        return Task.CompletedTask;
    }
    public Example01_MethodFunctions(ITestOutputHelper output) : base(output)
    {
    }
}
