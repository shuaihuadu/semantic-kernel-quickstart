IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile("appsettings.Development.json", true)
    .Build();

TestConfiguration.Initialize(configuration);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    string? xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});


builder.Services.AddTransient(providers =>
{
    return CreateSemanticKernel();
});

builder.Services.AddScoped<IAiPluginRunner, AiPluginRunner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


static Kernel CreateSemanticKernel()
{
    string defaultSemanticFunctionsFolder = "Plugins";

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

    kernel.Plugins.AddFromType<MathPlugin>();
    kernel.ImportPluginFromPromptDirectory(Path.Combine(AppContext.BaseDirectory, defaultSemanticFunctionsFolder, "FunPlugin"), "FunPlugin");

    return kernel;
}