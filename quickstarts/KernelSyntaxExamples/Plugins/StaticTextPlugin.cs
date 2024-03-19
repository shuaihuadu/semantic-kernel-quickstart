namespace KernelSyntaxExamples.Plugins;

public sealed class StaticTextPlugin
{
    [KernelFunction]
    public static string Uppercase([Description("Text to uppercase")] string input) => input.ToUpperInvariant();

    [KernelFunction]
    public static string AppendDay([Description("Text to append to")] string input, [Description("Value of the day to append")] string day) => input + day;
}
