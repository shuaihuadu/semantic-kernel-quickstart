namespace KernelSyntaxExamples;

public static class Example03_Arguments
{
    public static async Task RunAsync()
    {
        Console.WriteLine("======== Arguments ========");

        Kernel kernel = new();

        KernelPlugin textPlugin = kernel.ImportPluginFromType<StaticTextPlugin>();

        KernelArguments arguments = new()
        {
            ["input"] = "Today is: ",
            ["day"] = DateTimeOffset.Now.ToString("dddd", CultureInfo.CurrentCulture)
        };

        string? resultValue = await kernel.InvokeAsync<string>(textPlugin["AppendDay"], arguments);
        Console.WriteLine($"string -> {resultValue}");

        FunctionResult functionResult = await kernel.InvokeAsync(textPlugin["AppendDay"], arguments);

        var metadata = functionResult.Metadata;

        Console.WriteLine($"FunctionResult.GetValue<string>() -> {functionResult.GetValue<string>()}");

        Console.WriteLine($"FunctionResult.ToString() -> {functionResult}");
    }
}
