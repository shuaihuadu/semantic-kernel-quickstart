namespace AiPlugin.Functions;

public class TestPlugin
{
    private readonly ILogger _logger;
    private readonly IAiPluginRunner _pluginRunner;

    public TestPlugin(ILoggerFactory loggerFactory, IAiPluginRunner pluginRunner)
    {
        this._logger = loggerFactory.CreateLogger<TestPlugin>();
        this._pluginRunner = pluginRunner;
    }

    //[OpenApiOperation(operationId: "Limerick", tags: ["Limerick"], Description = "Generate a funny limerick about a person")]
    //[OpenApiParameter(name: "name", In = ParameterLocation.Query, Type = typeof(string))]
    //[OpenApiParameter(name: "input", In = ParameterLocation.Query, Type = typeof(string))]
    //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    //[Function("Limerick_Test")]
    //public Task<HttpResponseData> Limerick([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    //{
    //    this._logger.LogInformation("HTTP trigger processed a request for plugin FunPlugin - function Limerick.");

    //    return this._pluginRunner.RunAiPluginOperationAsync(request, "FunPlugin", "Limerick");
    //}

    //[OpenApiOperation(operationId: "Joke", tags: ["Joke"], Description = "Generate a funny joke")]
    //[OpenApiParameter(name: "input", Description = "Joke subject", In = ParameterLocation.Query, Type = typeof(string))]
    //[OpenApiParameter(name: "style", Description = "Give a hint about the desired joke style", In = ParameterLocation.Query, Type = typeof(string))]
    //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    //[Function("Joke_Test")]
    //public Task<HttpResponseData> Joke([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    //{
    //    this._logger.LogInformation("HTTP trigger processed a request for plugin FunPlugin - function Joke.");

    //    return this._pluginRunner.RunAiPluginOperationAsync(request, "FunPlugin", "Joke");
    //}

    //[OpenApiOperation(operationId: "Excuses", tags: ["Excuses"], Description = "Turn a scenario into a creative or humorous excuse to send your boss")]
    //[OpenApiParameter(name: "input", Description = "The event", In = ParameterLocation.Query, Type = typeof(string))]
    //[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
    //[Function("Excuses_Test")]
    //public Task<HttpResponseData> Excuses([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    //{
    //    this._logger.LogInformation("HTTP trigger processed a request for plugin FunPlugin - function Excuses.");

    //    return this._pluginRunner.RunAiPluginOperationAsync(request, "FunPlugin", "Excuses");
    //}
}