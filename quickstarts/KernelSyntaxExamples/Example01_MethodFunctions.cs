using Microsoft.SemanticKernel.Plugins.Core;

namespace KernelSyntaxExamples;

public static class Example01_MethodFunctions
{
    public static Task RunAsync()
    {
        Console.WriteLine("======== Functions ========");

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        TextPlugin textPlugin = new();

        string result = textPlugin.Uppercase("quick start");

        Console.WriteLine(result);

#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


        return Task.CompletedTask;
    }
}
