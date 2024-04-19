namespace KernelSyntaxExamples;

public class Example08_RetryHandler(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.Services.AddLogging(configure =>
        {
            configure.AddConsole().SetMinimumLevel(LogLevel.Information);
        });

        builder.Services.ConfigureHttpClientDefaults(configure1 =>
        {
            configure1.AddStandardResilienceHandler().Configure(configure2 =>
            {
                configure2.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.Unauthorized);
                // configure2.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is TimeoutRejectedException);
            });
        });

        builder.Services.AddOpenAIChatCompletion("gpt-4", "BAD_KEY");

        Kernel kernel = builder.Build();

        ILogger logger = kernel.LoggerFactory.CreateLogger(typeof(Example08_RetryHandler));

        const string Question = "How popular is the Polly library?";
        logger.LogInformation("Question: {Question}", Question);

        try
        {
            logger.LogInformation("Answer: {Result}", await kernel.InvokePromptAsync(Question));
        }
        catch (Exception ex)
        {
            logger.LogInformation("Error: {Message}", ex.Message);
        }
    }
}
