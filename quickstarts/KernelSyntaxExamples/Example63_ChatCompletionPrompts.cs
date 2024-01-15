namespace KernelSyntaxExamples;

public static class Example63_ChatCompletionPrompts
{
    public static async Task RunAsync()
    {
        const string ChatPrompt = @"
            <message role=""user"">What is Seattle?</message>
            <message role=""system"">Respond with JSON.</message>
        ";

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        KernelFunction chatSemanticFunction = kernel.CreateFunctionFromPrompt(ChatPrompt);

        FunctionResult chatPromptResult = await kernel.InvokeAsync(chatSemanticFunction);

        Console.WriteLine("Chat Prompt:");
        Console.WriteLine(ChatPrompt);
        Console.WriteLine("Chat Prompt Result:");
        Console.WriteLine(chatPromptResult.GetValue<string>());

        Console.Write("Chat Prompt Streaming Result:");

        await foreach (string message in kernel.InvokeStreamingAsync<string>(chatSemanticFunction))
        {
            Console.Write(message);
        }

        Console.WriteLine();
    }
}
