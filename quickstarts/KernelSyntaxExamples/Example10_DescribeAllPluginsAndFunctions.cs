﻿namespace KernelSyntaxExamples;

public static class Example10_DescribeAllPluginsAndFunctions
{
    public static Task RunAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.DeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        kernel.ImportPluginFromType<StaticTextPlugin>();

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        kernel.ImportPluginFromType<TextPlugin>();
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        string folder = RepoFiles.SamplePluginsPath();
        kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, "SummarizePlugin"));

        IList<KernelFunctionMetadata> kernelFunctionMetadatas = kernel.Plugins.GetFunctionsMetadata();

        Console.WriteLine("**********************************************");
        Console.WriteLine("****** Registered plugins and functions ******");
        Console.WriteLine("**********************************************");
        Console.WriteLine();

        foreach (KernelFunctionMetadata kernelFunctionMetadata in kernelFunctionMetadatas)
        {
            PrintFunction(kernelFunctionMetadata);
        }

        return Task.CompletedTask;
    }

    private static void PrintFunction(KernelFunctionMetadata kernelFunctionMetadata)
    {
        Console.WriteLine($"Plugin: {kernelFunctionMetadata.PluginName}");
        Console.WriteLine($"    {kernelFunctionMetadata.Name}: {kernelFunctionMetadata.Description}");

        if (kernelFunctionMetadata.Parameters.Count > 0)
        {
            Console.WriteLine("     Params:");

            foreach (var parameter in kernelFunctionMetadata.Parameters)
            {
                Console.WriteLine($"      - {parameter.Name}: {parameter.Description}");
                Console.WriteLine($"        default: '{parameter.DefaultValue}'");
            }
        }

        Console.WriteLine();
    }
}
