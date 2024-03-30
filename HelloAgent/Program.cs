IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json", true)
    .Build();

TestConfiguration.Initialize(configuration);

EmailAgent agent = new();

await agent.RunAsync();