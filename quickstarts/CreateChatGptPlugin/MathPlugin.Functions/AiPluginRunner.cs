using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace MathPlugin.Functions;

public class AiPluginRunner
{
    private readonly ILogger<AiPluginRunner> _logger;

    public AiPluginRunner(ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<AiPluginRunner>();
    }

    public async Task<HttpResponseData> RunAiPluginOperationAsync<T>(HttpRequestData request, string pluginName, string functionName)
    {
        Kernel kernel = Kernel.CreateBuilder().Build();

        KernelArguments arguments = ConvertToKernelArguments((await JsonSerializer.DeserializeAsync<T>(request.Body).ConfigureAwait(true))!);

        var response = request.CreateResponse(HttpStatusCode.OK);

        response.Headers.Add("Content-Type", "text/plain;charset=utf-8");

        FunctionResult result = await kernel.InvokeAsync(pluginName, functionName, arguments).ConfigureAwait(false);

        await response.WriteStringAsync(result.ToString()).ConfigureAwait(false);
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
}
