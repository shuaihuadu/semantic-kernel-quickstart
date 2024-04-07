namespace HomeAutomation;

internal sealed class Worker(IHostApplicationLifetime hostApplicationLifetime, [FromKeyedServices("HomeAutomationKernel")] Kernel kernel) : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
    private readonly Kernel _kernel = kernel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IChatCompletionService chatCompletionService = this._kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        Console.WriteLine("Ask questions or give instructions to the copilot such as:\n" +
                          "- What time is it?\n" +
                          "- Turn on the porch light.\n" +
                          "- If it's before 7:00 pm, turn on the office light.\n" +
                          "- Which light is currently on?\n" +
                          "- Set an alarm for 6:00 am.\n");

        Console.Write("> ");

        string? input = null;

        while ((input = Console.ReadLine()) != null)
        {
            Console.WriteLine();

            ChatMessageContent chatResult = await chatCompletionService.GetChatMessageContentAsync(
                input,
                openAIPromptExecutionSettings,
                this._kernel,
                stoppingToken);

            Console.WriteLine($"\n>>> Result: {chatResult}\n\n> ");
        }

        this._hostApplicationLifetime.StopApplication();
    }
}