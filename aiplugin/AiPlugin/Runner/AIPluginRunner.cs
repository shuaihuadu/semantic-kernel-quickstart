namespace AiPlugin.Runner;

public class AiPluginRunner : IAiPluginRunner
{
    private readonly ILogger<AiPluginRunner> _logger;
    private readonly Kernel _kernel;

    public AiPluginRunner(Kernel kernel, ILoggerFactory loggerFactory)
    {
        this._kernel = kernel;
        this._logger = loggerFactory.CreateLogger<AiPluginRunner>();
    }

    public Task<HttpResponseData> RunAIPluginOperationAsync(HttpRequestData request, string operationId)
    {
        throw new NotImplementedException();
    }

    protected static KernelArguments LoadKernelArgumentsFromRequest(HttpRequestData request)
    {
        KernelArguments kernelArguments = [];

        foreach (string? key in request.Query.AllKeys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                kernelArguments.Add(key, request.Query[key]);
            }
        }

        if (string.IsNullOrEmpty(request.Query.Get("input")))
        {
            string? body = request.ReadAsString();

            if (!string.IsNullOrEmpty(body))
            {
                kernelArguments["input"] = body;
            }
        }

        return kernelArguments;
    }
}
