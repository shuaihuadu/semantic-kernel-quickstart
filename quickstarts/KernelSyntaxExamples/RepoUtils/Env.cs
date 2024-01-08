namespace KernelSyntaxExamples.RepoUtils;

internal sealed class Env
{
    internal static string Var(string name)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddUserSecrets<Env>()
            .Build();

        string? value = configuration[name];

        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrEmpty(value))
        {
            throw new YourAppException($"Secret / Env var not set: {name}");
        }

        return value;
    }
}