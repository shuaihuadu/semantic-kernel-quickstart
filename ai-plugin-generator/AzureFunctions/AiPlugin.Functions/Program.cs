TestConfiguration.Initialize();

const string DefaultSemanticFunctionsFolder = "Plugins";

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((Action<IServiceCollection>)(services =>
    {
        services.AddTransient<Kernel>((Func<IServiceProvider, Kernel>)(providers =>
        {
            IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
                loggingBuilder.AddConsole();
            });

            Kernel kernel = builder.Build();

            kernel.Plugins.AddFromType<MathPlugin>();
            kernel.ImportPluginFromPromptDirectory(Path.Combine(DefaultSemanticFunctionsFolder, "FunPlugin"));
            kernel.ImportPluginFromPromptDirectory(Path.Combine(DefaultSemanticFunctionsFolder, "MiscPlugin"));

            return kernel;
        }))
        .AddScoped<IAiPluginRunner, AiPluginRunner>();
    }))
    .Build();

host.Run();
