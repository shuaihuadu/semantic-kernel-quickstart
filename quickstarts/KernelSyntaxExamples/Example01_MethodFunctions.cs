namespace KernelSyntaxExamples;

public static class Example01_MethodFunctions
{
    public static Task RunAsync()
    {
        Console.WriteLine("======== Functions ========");

        TextPlugin textPlugin = new();

        string result = textPlugin.Uppercase("quick start");

        Console.WriteLine(result);

        return Task.CompletedTask;
    }
}
