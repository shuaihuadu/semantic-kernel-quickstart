namespace KernelSyntaxExamples;

public class Example35_GrpcPlugins : BaseTest
{
    [Fact(Skip = "Setup crendentials")]
    public async Task RunAsync()
    {
        Kernel kernel = new();

        KernelPlugin plugin = kernel.ImportPluginFromGrpcFile("<path-to-.proto-file>", "<plugin-name>");

        KernelArguments arguments = new();
        arguments["address"] = "<gRPC-server-address>";
        arguments["payload"] = "<gRPC-request-message-as-json>";

        FunctionResult result = await kernel.InvokeAsync(plugin["<operation-name>"], arguments);

        Console.WriteLine("Plugin response: {0}", result.GetValue<string>());
    }

    public Example35_GrpcPlugins(ITestOutputHelper output) : base(output)
    {
    }
}
