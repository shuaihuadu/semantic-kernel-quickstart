namespace AiPlugin.Web.Runner;

public interface IAiPluginRunner
{
    Task<string> RunAiPluginOperationAsync(HttpRequest request, string pluginName, string functionName);
}
