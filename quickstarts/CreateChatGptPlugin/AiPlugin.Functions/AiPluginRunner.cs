namespace AiPlugin.Functions;

public class AiPluginRunner(Kernel kernel, ILoggerFactory loggerFactory)
{
    private readonly ILogger<AiPluginRunner> _logger = loggerFactory.CreateLogger<AiPluginRunner>();
    private readonly Kernel _kernel = kernel;

    public async Task<HttpResponseData> RunAiPluginOperationAsync<T>(HttpRequestData request, string pluginName, string functionName)
    {
        KernelArguments arguments = ConvertToKernelArguments((await JsonSerializer.DeserializeAsync<T>(request.Body).ConfigureAwait(true))!);

        var response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain;charset=utf-8");
        await response.WriteStringAsync(
            (await this._kernel.InvokeAsync(pluginName, functionName, arguments).ConfigureAwait(false)).ToString()
        ).ConfigureAwait(false);
        return response;
    }

    private static KernelArguments ConvertToKernelArguments<T>(T model)
    {
        {
            var arguments = new KernelArguments();
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (property.GetValue(model) != null)
                {
                    arguments.Add(property.Name, property.GetValue(model));
                }
            }
            return arguments;
        }
    }

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
