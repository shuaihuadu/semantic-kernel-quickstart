namespace AIPluginFunction;

public static class Logo
{
    [Function("GetLogo")]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "logo.png")] HttpRequestData request)
    {
        HttpResponseData response = request.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "image/png");

        byte[] logo = File.ReadAllBytes("logo.png");

        response.Body.Write(logo);

        return response;
    }
}
