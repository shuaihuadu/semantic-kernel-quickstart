using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;

namespace AIPluginFunction.Runner;

public interface IAIPluginRunner
{
    Task<HttpResponseData> RunAIPluginOperationAsync(HttpRequestData request, string operationId);
}
