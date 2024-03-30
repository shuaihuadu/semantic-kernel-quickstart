IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.Development.json", true)
    .Build();

TestConfiguration.Initialize(configuration);

Agent agent = new();

await agent.RunAsync();