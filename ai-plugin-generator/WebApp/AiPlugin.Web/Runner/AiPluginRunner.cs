namespace AiPlugin.Web.Runner;

public class AiPluginRunner(Kernel kernel, ILoggerFactory loggerFactory) : IAiPluginRunner
{
    private readonly ILogger<AiPluginRunner> _logger = loggerFactory.CreateLogger<AiPluginRunner>();
    private readonly Kernel _kernel = kernel;

    public async Task<string> RunAiPluginOperationAsync(HttpRequest request, string pluginName, string functionName)
    {
        KernelArguments arguments = LoadKernelArgumentsFromRequest(request);

        if (!this._kernel.Plugins.TryGetFunction(pluginName, functionName, out KernelFunction? kernelFunction))
        {
            return $"Function {functionName} not found";
        }

        FunctionResult result = await this._kernel.InvokeAsync(kernelFunction, arguments);

        return result.ToString();
    }

    protected static KernelArguments LoadKernelArgumentsFromRequest(HttpRequest request)
    {
        KernelArguments kernelArguments = [];

        foreach (string? key in request.Query.Keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                kernelArguments.Add(key, request.Query[key]);
            }
        }

        return kernelArguments;
    }
}
