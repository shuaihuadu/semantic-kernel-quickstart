using AiPluginFunctionGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace AiPluginSourceGenerator
{

    [Generator]
    public class AiPluginFunctionGenerator : ISourceGenerator
    {
        private const string DefaultFunctionNamespace = "AiPlugin";
        private const string FunctionConfigFileName = "config.json";
        private const string FunctionPromptFileName = "skprompt.txt";

        public void Execute(GeneratorExecutionContext context)
        {
            string? rootNamespace = context.GetRootNameSpace();

            if (string.IsNullOrEmpty(rootNamespace))
            {
                rootNamespace = DefaultFunctionNamespace;
            }

            IEnumerable<AdditionalText> functionFiles = context.AdditionalFiles.Where(f =>
            f.Path.Contains(FunctionConfigFileName) ||
            f.Path.Contains(FunctionPromptFileName));

            IEnumerable<IGrouping<string?, AdditionalText>> fnFileGroup = functionFiles.GroupBy(f => Path.GetDirectoryName(f.Path));
            IEnumerable<IGrouping<string?, IGrouping<string?, AdditionalText>>> folderGroups = fnFileGroup.GroupBy(f => Path.GetFileName(Path.GetDirectoryName(f.Key)));

            foreach (var folderGroup in folderGroups)
            {
                string? folderName = folderGroup.Key;

                if (string.IsNullOrWhiteSpace(folderName))
                {
                    continue;
                }

                string classSource = GenerateClassSource(rootNamespace!, folderName, folderGroup);

                context.AddSource(folderName, SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string GenerateClassSource(string rootNamespace, string folderName, IGrouping<string?, IGrouping<string?, AdditionalText>> folderGroup)
        {
            StringBuilder functionsCode = new StringBuilder();

            foreach (var functionGroup in folderGroup)
            {
                AdditionalText? promptFile = functionGroup.FirstOrDefault(f => Path.GetFileName(f.Path).Equals(FunctionPromptFileName, StringComparison.InvariantCultureIgnoreCase));
                AdditionalText? configFile = functionGroup.FirstOrDefault(f => Path.GetFileName(f.Path).Equals(FunctionConfigFileName, StringComparison.InvariantCultureIgnoreCase));

                if (promptFile != default && configFile != default)
                {
                    string code = GenerateFunctionSource(promptFile, configFile) ?? string.Empty;
                    functionsCode.AppendLine();
                }
            }

            return $@"/* ### GENERATED CODE - Do not modify. Edits will be lost on build. ### */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace {rootNamespace};

public class {folderName}
{{
    private readonly ILogger _logger;
    private readonly IAiPluginRunner _pluginRunner;

    public {folderName}(ILoggerFactory loggerFactory, IAiPluginRunner pluginRunner)
    {{
        this._logger = loggerFactory.CreateLogger<{folderName}>();
        this._pluginRunner = pluginRunner;
    }}

    {functionsCode}
}}";
        }

        private static string? GenerateFunctionSource(AdditionalText promptFile, AdditionalText configFile)
        {
            string? functionName = Path.GetFileName(Path.GetDirectoryName(promptFile.Path));

            if (string.IsNullOrWhiteSpace(functionName))
            {
                return null;
            }

            string? metadataJson = configFile.GetText()?.ToString();

            if (string.IsNullOrEmpty(metadataJson))
            {
                return null;
            }

            PromptTemplateConfig? promptTemplateConfig = JsonSerializer.Deserialize<PromptTemplateConfig>(metadataJson);

            if (promptTemplateConfig is null) { return null; }

            string descriptionProperty = string.IsNullOrWhiteSpace(promptTemplateConfig.Description) ? string.Empty : $@", Description = ""{promptTemplateConfig.Description}""";

            string parameterAttributes = GenerateParameterAttributesSource(promptTemplateConfig.InputVariables);

            return $@"
    [OpenApiOperation(operationId: ""{functionName}"", tags: new []{{ ""{functionName}"" }}{descriptionProperty})]{parameterAttributes}
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ""text/plain"", bodyType: typeof(string), Description = ""The OK response"")]
    [Function(""{functionName}"")]
    public Task<HttpResponseData> {functionName}([HttpTrigger](AuthorizationLevel.Anonymous, ""post"")] HttpRequestData request)
    {{
        this._logger.LogInformation(""HTTP trigger processed a request for function {functionName}."");

        return this._pluginRunner.RunAIPluginOperationAsync(request,""{functionName}"");
    }}";
        }

        private static string GenerateParameterAttributesSource(List<InputVariable> inputVariables)
        {
            string inputDescription = string.Empty;

            StringBuilder parameterStringBuilder = new StringBuilder();

            if (inputVariables != null)
            {
                foreach (InputVariable inputVariable in inputVariables)
                {
                    parameterStringBuilder.AppendLine();
                    parameterStringBuilder.Append($@"   [OpenApiParameter(name: ""{inputVariable.Name}""]");

                    if (!string.IsNullOrWhiteSpace(inputVariable.Description))
                    {
                        parameterStringBuilder.Append($@", Description = ""{inputVariable.Description}""");
                    }

                    parameterStringBuilder.Append(", In = ParameterLocation.Query");
                    parameterStringBuilder.Append(", Type = typeof(string))");
                }
            }

            return parameterStringBuilder.ToString();
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}