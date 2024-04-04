Env.LoadUserSecrets();

IKernelBuilder builder = Kernel.CreateBuilder();

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
    endpoint: TestConfiguration.AzureOpenAI.Endpoint,
    apiKey: TestConfiguration.AzureOpenAI.ApiKey);

Kernel kernel = builder.Build();

await kernel.ImportPluginFromOpenApiAsync("MathPlugin", new Uri("http://localhost:7181/api/swagger.json")).ConfigureAwait(false);


Console.WriteLine("Please input a number:");

while (true)
{
    double number1 = Convert.ToDouble(Console.ReadLine());

    FunctionResult sqResult = await kernel.InvokeAsync("MathPlugin", "Sqrt", new()
    {
        ["number1"] = number1
    });

    Console.WriteLine(sqResult);
}