const string DefaultSemanticFunctionsFolder = "Prompts";

string semanticFunctionsFolder = Environment.GetEnvironmentVariable("SEMANTIC_SKILLS_FOLDER")
    ?? DefaultSemanticFunctionsFolder;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped(providers =>
        {
            Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
               deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
               endpoint: TestConfiguration.AzureOpenAI.Endpoint,
               apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

            kernel.ImportPluginFromPromptDirectory(semanticFunctionsFolder);

            return kernel;
        })
        .AddScoped<IAIPluginRunner, AIPluginRunner>();
    })
    .Build();