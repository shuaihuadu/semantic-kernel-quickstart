namespace Filtering;

public class RetryWithFilters(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task ChangeModelAndRetryAsync()
    {
        const string DefaultModelId = "defaultModelId";
        const string FallbackModelId = "FallbackModelId";

        IKernelBuilder builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            apiKey: "invalid_key",
            modelId: DefaultModelId);

        builder.AddAzureOpenAIChatCompletion(
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey,
            modelId: FallbackModelId);

        builder.Services.AddSingleton<IFunctionInvocationFilter>(new RetryFilter(FallbackModelId));

        Kernel kernel = builder.Build();

        OpenAIPromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings
        {
            ModelId = DefaultModelId,
            MaxTokens = 20
        };

        FunctionResult result = await kernel.InvokePromptAsync("Hi, can you help me today?", new(executionSettings));

        Console.WriteLine(result);

    }

    private class RetryFilter(string fallbackModelId) : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            try
            {
                await next(context);
            }
            catch (HttpOperationException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
            {
                PromptExecutionSettings executionSettings = context.Arguments.ExecutionSettings![PromptExecutionSettings.DefaultServiceId];

                executionSettings.ModelId = fallbackModelId;

                await next(context);
            }
        }
    }
}
