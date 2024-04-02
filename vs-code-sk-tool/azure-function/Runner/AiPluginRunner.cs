using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Net;

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

    public async Task<HttpResponseData> RunAIPluginOperationAsync(HttpRequestData request, string operationId)
    {
        KernelArguments arguments = LoadKernelArgumentsFromRequest(request);

        if (!this._kernel.Plugins.TryGetFunction(operationId, TestConfiguration.AiPluginSettings.NameForModel, out KernelFunction? kernelFunction))
        {
            HttpResponseData errorResponse = request.CreateResponse(HttpStatusCode.NotFound);

            await errorResponse.WriteStringAsync($"Function {operationId} not found");

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
