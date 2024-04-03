namespace AiPlugin.Runner;

public interface IAiPluginRunner
{
    Task<HttpResponseData> RunAiPluginOperationAsync(HttpRequestData request, string pluginName, string functionName);
}
