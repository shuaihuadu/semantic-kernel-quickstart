using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading.Tasks;

namespace MathPlugin.Functions;

public class MathPlugin
{
    private readonly ILogger _logger;
    private readonly AiPluginRunner _pluginRunner;

    public MathPlugin(ILoggerFactory loggerFactory, AiPluginRunner pluginRunner)
    {
        _logger = loggerFactory.CreateLogger<MathPlugin>();
        _pluginRunner = pluginRunner;
    }


    public class SqrtModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Sqrt", tags: new[] { "Sqrt" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SqrtModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Sqrt")]
    public async Task<HttpResponseData> Sqrt([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Sqrt.");
        return await _pluginRunner.RunAiPluginOperationAsync<SqrtModel>(req, "MathPlugin", "Sqrt");
    }

    public class AddModel
    {
        public double number1 { get; set; }
        public double number2 { get; set; }
    }


    [OpenApiOperation(operationId: "Add", tags: new[] { "Add" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AddModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Add")]
    public async Task<HttpResponseData> Add([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Add.");
        return await _pluginRunner.RunAiPluginOperationAsync<AddModel>(req, "MathPlugin", "Add");
    }

    public class SubtractModel
    {
        public double number1 { get; set; }
        public double number2 { get; set; }
    }


    [OpenApiOperation(operationId: "Subtract", tags: new[] { "Subtract" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SubtractModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Subtract")]
    public async Task<HttpResponseData> Subtract([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Subtract.");
        return await _pluginRunner.RunAiPluginOperationAsync<SubtractModel>(req, "MathPlugin", "Subtract");
    }

    public class MultiplyModel
    {
        public double number1 { get; set; }
        public double number2 { get; set; }
    }


    [OpenApiOperation(operationId: "Multiply", tags: new[] { "Multiply" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(MultiplyModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Multiply")]
    public async Task<HttpResponseData> Multiply([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Multiply.");
        return await _pluginRunner.RunAiPluginOperationAsync<MultiplyModel>(req, "MathPlugin", "Multiply");
    }

    public class DivideModel
    {
        public double number1 { get; set; }
        public double number2 { get; set; }
    }


    [OpenApiOperation(operationId: "Divide", tags: new[] { "Divide" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DivideModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Divide")]
    public async Task<HttpResponseData> Divide([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Divide.");
        return await _pluginRunner.RunAiPluginOperationAsync<DivideModel>(req, "MathPlugin", "Divide");
    }

    public class PowerModel
    {
        public double number1 { get; set; }
        public double number2 { get; set; }
    }


    [OpenApiOperation(operationId: "Power", tags: new[] { "Power" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(PowerModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Power")]
    public async Task<HttpResponseData> Power([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Power.");
        return await _pluginRunner.RunAiPluginOperationAsync<PowerModel>(req, "MathPlugin", "Power");
    }

    public class LogModel
    {
        public double number1 { get; set; }
        public double number2 { get; set; }
    }


    [OpenApiOperation(operationId: "Log", tags: new[] { "Log" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LogModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Log")]
    public async Task<HttpResponseData> Log([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Log.");
        return await _pluginRunner.RunAiPluginOperationAsync<LogModel>(req, "MathPlugin", "Log");
    }

    public class RoundModel
    {
        public double number1 { get; set; }
        public double number2 { get; set; }
    }


    [OpenApiOperation(operationId: "Round", tags: new[] { "Round" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RoundModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Round")]
    public async Task<HttpResponseData> Round([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Round.");
        return await _pluginRunner.RunAiPluginOperationAsync<RoundModel>(req, "MathPlugin", "Round");
    }

    public class AbsModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Abs", tags: new[] { "Abs" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AbsModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Abs")]
    public async Task<HttpResponseData> Abs([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Abs.");
        return await _pluginRunner.RunAiPluginOperationAsync<AbsModel>(req, "MathPlugin", "Abs");
    }

    public class FloorModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Floor", tags: new[] { "Floor" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(FloorModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Floor")]
    public async Task<HttpResponseData> Floor([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Floor.");
        return await _pluginRunner.RunAiPluginOperationAsync<FloorModel>(req, "MathPlugin", "Floor");
    }

    public class CeilingModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Ceiling", tags: new[] { "Ceiling" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CeilingModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Ceiling")]
    public async Task<HttpResponseData> Ceiling([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Ceiling.");
        return await _pluginRunner.RunAiPluginOperationAsync<CeilingModel>(req, "MathPlugin", "Ceiling");
    }

    public class SinModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Sin", tags: new[] { "Sin" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SinModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Sin")]
    public async Task<HttpResponseData> Sin([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Sin.");
        return await _pluginRunner.RunAiPluginOperationAsync<SinModel>(req, "MathPlugin", "Sin");
    }

    public class CosModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Cos", tags: new[] { "Cos" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CosModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Cos")]
    public async Task<HttpResponseData> Cos([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Cos.");
        return await _pluginRunner.RunAiPluginOperationAsync<CosModel>(req, "MathPlugin", "Cos");
    }

    public class TanModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Tan", tags: new[] { "Tan" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TanModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Tan")]
    public async Task<HttpResponseData> Tan([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Tan.");
        return await _pluginRunner.RunAiPluginOperationAsync<TanModel>(req, "MathPlugin", "Tan");
    }

    public class AsinModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Asin", tags: new[] { "Asin" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AsinModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Asin")]
    public async Task<HttpResponseData> Asin([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Asin.");
        return await _pluginRunner.RunAiPluginOperationAsync<AsinModel>(req, "MathPlugin", "Asin");
    }

    public class AcosModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Acos", tags: new[] { "Acos" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AcosModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Acos")]
    public async Task<HttpResponseData> Acos([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Acos.");
        return await _pluginRunner.RunAiPluginOperationAsync<AcosModel>(req, "MathPlugin", "Acos");
    }

    public class AtanModel
    {
        public double number1 { get; set; }
    }


    [OpenApiOperation(operationId: "Atan", tags: new[] { "Atan" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(AtanModel), Required = true, Description = "JSON request body")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    [FunctionName("Atan")]
    public async Task<HttpResponseData> Atan([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger processed a request for function MathPlugin-Atan.");
        return await _pluginRunner.RunAiPluginOperationAsync<AtanModel>(req, "MathPlugin", "Atan");
    }

}