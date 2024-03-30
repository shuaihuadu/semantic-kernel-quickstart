namespace AiPlugin;

public class Logo
{
    [Function("GetLogo")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "logo.png")] HttpRequestData request)
    {
        HttpResponseData response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "image/png");

        byte[] logo = File.ReadAllBytes("logo.png");

        await response.Body.WriteAsync(logo);

        return response;
    }
}
