internal sealed class Env
{
    internal static void LoadUserSecrets()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", true)
            .Build();

        TestConfiguration.Initialize(configurationRoot);
    }
}
