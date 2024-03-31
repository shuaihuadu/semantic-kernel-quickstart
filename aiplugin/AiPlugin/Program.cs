IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile("appsettings.Development.json", true)
    .Build();

TestConfiguration.Initialize(configuration);

const string DefaultSemanticFunctionsFolder = "Prompts";

string semanticFunctionsFolder = Environment.GetEnvironmentVariable("SEMANTIC_SKILLS_FOLDER")
    ?? DefaultSemanticFunctionsFolder;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
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

            builder.Plugins.AddFromType<MathPlugin>();

            Kernel kernel = builder.Build();


            kernel.ImportPluginFromPromptDirectory(semanticFunctionsFolder);

            return kernel;
        })
        .AddScoped<IAiPluginRunner, AiPluginRunner>();
    })
    .Build();

host.Run();
