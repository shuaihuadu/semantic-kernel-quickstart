using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MathPlugin.Functions;

public class MathPlugin
{
    private readonly ILogger _logger;

    public MathPlugin(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MathPlugin>();
    }

    public class SqrtModel
    {
        public double number1 { get; set; }
    }

    [OpenApiOperation(operationId: "Sqrt", tags: new[] { "Sqrt" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SqrtModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Sqrt")]
    public Task<string> Sqrt([HttpTrigger(AuthorizationLevel.Anonymous, "post")] string data)
    {
        SqrtModel model = JsonSerializer.Deserialize<SqrtModel>(data);

        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Sqrt.");

        double squareRoot = Math.Sqrt(model.number1);

        return Task.FromResult($"Result from MathPlugin in Azure Functions:The square root of {model.number1} is {squareRoot}");
    }
}