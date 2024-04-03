namespace AiPlugin.Runner;

public class AiPluginRunner(Kernel kernel, ILoggerFactory loggerFactory) : IAiPluginRunner
{
    private readonly ILogger<AiPluginRunner> _logger = loggerFactory.CreateLogger<AiPluginRunner>();
    private readonly Kernel _kernel = kernel;

    public async Task<HttpResponseData> RunAiPluginOperationAsync(HttpRequestData request, string pluginName, string functionName)
    {
        KernelArguments arguments = LoadKernelArgumentsFromRequest(request);

        if (!this._kernel.Plugins.TryGetFunction(pluginName, functionName, out KernelFunction? kernelFunction))
        {
            HttpResponseData errorResponse = request.CreateResponse(HttpStatusCode.NotFound);

            await errorResponse.WriteStringAsync($"Function {functionName} not found");

            return errorResponse;
        }

        FunctionResult result = await this._kernel.InvokeAsync(kernelFunction, arguments);

        HttpResponseData response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain;charset=utf-8");

        await response.WriteStringAsync(result.ToString());

        return response;
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
