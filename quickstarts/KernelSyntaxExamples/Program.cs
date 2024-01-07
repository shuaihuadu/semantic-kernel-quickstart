using KernelSyntaxExamples;

const string filter = "";

using CancellationTokenSource cts = new();
CancellationToken cancellationToken = cts.ConsoleCancellationToken();
await RunExamplesAsync(filter, cancellationToken);

static async Task RunExamplesAsync(string? filter, CancellationToken cancellationToken)
{
    await Example01_MethodFunctions.RunAsync();
}

public static class Extensions
{
    public static CancellationToken ConsoleCancellationToken(this CancellationTokenSource tokenSource)
    {
        Console.CancelKeyPress += (s, e) =>
        {
            Console.WriteLine("Canceling...");
            tokenSource.Cancel();
            e.Cancel = true;
        };

        return tokenSource.Token;
    }
}
