namespace AiPlugin.Runner;

public interface IAiPluginRunner
{
    Task<HttpResponseData> RunAIPluginOperationAsync(HttpRequestData request, string operationId);
}
