IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile("appsettings.Development.json", true)
    .Build();

TestConfiguration.Initialize(configuration);

const string DefaultSemanticFunctionsFolder = "Plugins";

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddTransient(providers =>
        {
            IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
               deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
               endpoint: TestConfiguration.AzureOpenAI.Endpoint,
               apiKey: TestConfiguration.AzureOpenAI.ApiKey);

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
                loggingBuilder.AddConsole();
            });

            Kernel kernel = builder.Build();

            kernel.ImportPluginFromPromptDirectory(DefaultSemanticFunctionsFolder);

            return kernel;
        })
        .AddScoped<IAiPluginRunner, AiPluginRunner>();
    })
    .Build();

host.Run();
