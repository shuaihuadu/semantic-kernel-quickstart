namespace KernelSyntaxExamples;

public class Example35_GrpcPlugins(ITestOutputHelper output) : BaseTest(output)
{
    [Fact(Skip = "Setup crendentials")]
    public async Task RunAsync()
    {
        Kernel kernel = new();

        KernelPlugin plugin = kernel.ImportPluginFromGrpcFile("<path-to-.proto-file>", "<plugin-name>");

        KernelArguments arguments = new()
        {
            ["address"] = "<gRPC-server-address>",
            ["payload"] = "<gRPC-request-message-as-json>"
        };

        FunctionResult result = await kernel.InvokeAsync(plugin["<operation-name>"], arguments);

        WriteLine($"Plugin response: {result.GetValue<string>()}");
    }
}
