using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace AiPlugin.Plugins;

public class AFunPlugin
{
    private readonly ILogger _logger;
    private readonly IAiPluginRunner _pluginRunner;

    public AFunPlugin(ILoggerFactory loggerFactory, IAiPluginRunner pluginRunner)
    {
        this._logger = loggerFactory.CreateLogger<AFunPlugin>();
        this._pluginRunner = pluginRunner;
    }


    [OpenApiOperation(operationId: "ElementAtIndex", tags: new[] { "ElementAtIndex" }, Description = "Get an element from an array or list at a specified index")]
    [OpenApiParameter(name: "input", Description = "The input array or list", In = ParameterLocation.Query, Type = typeof(string))]
    [OpenApiParameter(name: "index", Description = "The index of the element to retrieve", In = ParameterLocation.Query, Type = typeof(string))]
    [OpenApiParameter(name: "count", Description = "The number of items in the input", In = ParameterLocation.Query, Type = typeof(string))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    [Function("ElementAtIndex")]
    public Task<HttpResponseData> ElementAtIndex([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        this._logger.LogInformation("HTTP trigger processed a request for function ElementAtIndex.");

        return this._pluginRunner.RunAIPluginOperationAsync(request, "ElementAtIndex");
    }

    [OpenApiOperation(operationId: "Continue", tags: new[] { "Continue" }, Description = "Given a text input, continue it with additional text.")]
    [OpenApiParameter(name: "input", Description = "The text to continue.", In = ParameterLocation.Query, Type = typeof(string))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    [Function("Continue")]
    public Task<HttpResponseData> Continue([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        this._logger.LogInformation("HTTP trigger processed a request for function Continue.");

        return this._pluginRunner.RunAIPluginOperationAsync(request, "Continue");
    }

}