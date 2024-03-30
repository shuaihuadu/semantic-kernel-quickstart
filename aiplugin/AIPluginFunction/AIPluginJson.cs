namespace AIPluginFunction;

public class AIPluginJson
{
    [Function("GetAIPluginJson")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/ai-plugin.json")] HttpRequestData request)
    {
        string currentDomain = $"{request.Url.Scheme}://{request.Url.Host}:{request.Url.Port}";

        HttpResponseData response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        //TODO appsettings.json

        return response;
    }
}
