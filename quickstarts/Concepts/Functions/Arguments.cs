namespace Functions;

public class Arguments(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Arguments ========");

        Kernel kernel = new();

        KernelPlugin textPlugin = kernel.ImportPluginFromType<StaticTextPlugin>();

        KernelArguments arguments = new()
        {
            ["input"] = "Today is: ",
            ["day"] = DateTimeOffset.Now.ToString("dddd", CultureInfo.CurrentCulture)
        };

        string? resultValue = await kernel.InvokeAsync<string>(textPlugin["AppendDay"], arguments);
        this.WriteLine($"string -> {resultValue}");

        FunctionResult functionResult = await kernel.InvokeAsync(textPlugin["AppendDay"], arguments);

        var metadata = functionResult.Metadata;

        this.WriteLine($"FunctionResult.GetValue<string>() -> {functionResult.GetValue<string>()}");

        this.WriteLine($"FunctionResult.ToString() -> {functionResult}");
    }
}
