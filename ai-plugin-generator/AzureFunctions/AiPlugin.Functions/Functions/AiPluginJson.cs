namespace AiPlugin.Functions;

public class AiPluginJson
{
    private readonly ILogger<AiPluginJson> _logger;

    public AiPluginJson(ILogger<AiPluginJson> logger)
    {
        _logger = logger;
    }

    [Function("GetAiPluginJson")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = ".well-known/ai-plugin.json")] HttpRequestData request)
    {
        var currentDomain = $"{request.Url.Scheme}://{request.Url.Host}:{request.Url.Port}";

        HttpResponseData response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        string json = JsonSerializer.Serialize(new AiPluginSettings());

        json = json.Replace("{url}", currentDomain, StringComparison.OrdinalIgnoreCase);

        await response.WriteStringAsync(json);

        return response;
    }
}
