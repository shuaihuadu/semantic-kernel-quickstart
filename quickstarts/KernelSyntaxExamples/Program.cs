using KernelSyntaxExamples;

const string filter = "Example06_TemplateLanguage";

LoadUserSecrets();

using CancellationTokenSource cts = new();
CancellationToken cancellationToken = cts.ConsoleCancellationToken();

await RunExamplesAsync(filter, cancellationToken);

Console.ReadKey();

static async Task RunExamplesAsync(string? filter, CancellationToken cancellationToken)
{
    List<string> examples = (Assembly.GetExecutingAssembly().GetTypes())
        .Select(type => string.IsNullOrEmpty(type.FullName) ? string.Empty : type.FullName)
        .Where(type => !string.IsNullOrEmpty(type))
        .ToList();

    foreach (var example in examples)
    {
        if (string.IsNullOrEmpty(filter) || example.Contains(filter, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                MethodInfo? method = Assembly.GetExecutingAssembly().GetType(example)?.GetMethod("RunAsync");

                if (method == null)
                {
                    continue;
                }

                Console.WriteLine($"Running {example}...");

                bool hasCancellationToken = method.GetParameters().Any(param => param.ParameterType == typeof(CancellationToken));

                object[]? taskParameters = hasCancellationToken ? [cancellationToken] : null;

                if (method.Invoke(null, taskParameters) is Task t)
                {
                    await t.SafeWaitAsync(cancellationToken);
                }
                else
                {
                    method.Invoke(null, null);
                }
            }
            catch (ConfigurationNotFoundException ex)
            {
                Console.WriteLine($"{ex.Message}. Skipping example {example}.");
            }
        }
    }
}

static void LoadUserSecrets()
{
    IConfigurationRoot configRoot = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Development.json", true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Env>()
        .Build();

    TestConfiguration.Initialize(configRoot);
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

    public static async Task SafeWaitAsync(this Task task, CancellationToken cancellationToken = default)
    {
        try
        {
            await task.WaitAsync(cancellationToken);

            Console.WriteLine();

            Console.WriteLine("=== DONE ===");
        }
        catch (ConfigurationNotFoundException ex)
        {
            Console.WriteLine($"{ex.Message}. Skipping examples.");
        }

        cancellationToken.ThrowIfCancellationRequested();
    }
}
