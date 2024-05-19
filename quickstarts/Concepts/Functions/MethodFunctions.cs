namespace Functions;

public class MethodFunctions(ITestOutputHelper output) : BaseTest(output)
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
}
