// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using StepwisePlannerMigration.Extensions;
using StepwisePlannerMigration.Options;
using StepwisePlannerMigration.Services;

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot configRoot = new ConfigurationBuilder()
    .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
    .Build();

AzureOpenAIOptions azureOpenAIOptions = configRoot.GetValid<AzureOpenAIOptions>(AzureOpenAIOptions.SectionName);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
builder.Services.AddTransient<IPlanProvider, PlanProvider>();

builder.Services.AddKernel();
builder.Services.AddAzureOpenAIChatCompletion(deploymentName: azureOpenAIOptions.DeploymentName, endpoint: azureOpenAIOptions.Endpoint, apiKey: azureOpenAIOptions.ApiKey);

builder.Services.AddTransient<FunctionCallingStepwisePlanner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
