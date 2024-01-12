namespace KernelSyntaxExamples;

public static class Example35_GrpcPlugins
{
    public static async Task RunAsync()
    {
        Kernel kernel = new();

        KernelPlugin plugin = kernel.ImportPluginFromGrpcFile("<path-to-.proto-file>", "<plugin-name>");

        KernelArguments arguments = new();
        arguments["address"] = "<gRPC-server-address>";
        arguments["payload"] = "<gRPC-request-message-as-json>";

        FunctionResult result = await kernel.InvokeAsync(plugin["<operation-name>"], arguments);

        Console.WriteLine("Plugin response: {0}", result.GetValue<string>());
    }
}
