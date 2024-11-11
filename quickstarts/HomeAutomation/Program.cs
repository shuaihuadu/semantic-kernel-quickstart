using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace HomeAutomation;

internal class Program
{
    internal static async Task Main(string[] args)
    {
        TestConfiguration.Initialize();

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<Worker>();

        builder.Services.AddSingleton<IChatCompletionService>(sp =>
        {
            return new AzureOpenAIChatCompletionService(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey);
        });

        builder.Services.AddSingleton<MyTimePlugin>();
        builder.Services.AddSingleton<MyAlarmPlugin>();
        builder.Services.AddKeyedSingleton<MyLightPlugin>("OfficeLight");
        builder.Services.AddKeyedSingleton("PorchLight", (sp, key) =>
        {
            return new MyLightPlugin(turnOn: true);
        });


        builder.Services.AddKeyedTransient<Kernel>("HomeAutomationKernel", (sp, key) =>
        {
            KernelPluginCollection pluginCollection = new();

            pluginCollection.AddFromObject(sp.GetRequiredService<MyTimePlugin>());
            pluginCollection.AddFromObject(sp.GetRequiredService<MyAlarmPlugin>());
            pluginCollection.AddFromObject(sp.GetRequiredKeyedService<MyLightPlugin>("OfficeLight"), "OfficeLight");
            pluginCollection.AddFromObject(sp.GetRequiredKeyedService<MyLightPlugin>("PorchLight"), "PorchLight");

            return new Kernel(sp, pluginCollection);
        });

        using IHost host = builder.Build();

        await host.RunAsync();
    }
}
