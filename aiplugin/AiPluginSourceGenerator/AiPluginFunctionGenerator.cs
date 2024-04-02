﻿using AiPluginFunctionGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace AiPluginSourceGenerator;

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

        //获取所有Build Action属性设置为C# analyzer additional file的文件，并按照其所在的目录进行分组
        IEnumerable<IGrouping<string?, AdditionalText>> functionFileGroups = functionFiles.GroupBy(f => Path.GetDirectoryName(f.Path));

        //再按照具体的插件进行分组，获取Function所属的Plugin的目录，譬如：Plugins目录下面有APlugin、BPlugin，此处获取的是以APlugin和BPlugin为Key的分组结果
        IEnumerable<IGrouping<string?, IGrouping<string?, AdditionalText>>> pluginFolderGroups = functionFileGroups.GroupBy(f => Path.GetFileName(Path.GetDirectoryName(f.Key)));

        foreach (var pluginFolder in pluginFolderGroups)
        {
            string? pluginName = pluginFolder.Key;

            string pluginNamespace = $"{rootNamespace}.{pluginName}";

            foreach (var functionGroups in pluginFolder.ToList())
            {
                string classSource = GenerateClassSource(pluginNamespace, functionGroups!);

                context.AddSource(pluginName!, SourceText.From(classSource, Encoding.UTF8));
            }
        }
    }

    private string GenerateClassSource(string pluginNamespace, IGrouping<string, IGrouping<string, AdditionalText>> functionGroups)
    {
        StringBuilder functionsCode = new();

        string functionName = Path.GetFileName(functionGroups.Key);

        AdditionalText? promptFile = functionGroups.FirstOrDefault(f => Path.GetFileName(f.Path).Equals(FunctionPromptFileName, StringComparison.InvariantCultureIgnoreCase));
        AdditionalText? configFile = functionGroups.FirstOrDefault(f => Path.GetFileName(f.Path).Equals(FunctionConfigFileName, StringComparison.InvariantCultureIgnoreCase));

        if (promptFile != default && configFile != default)
        {
            string code = GenerateFunctionSource(promptFile, configFile) ?? string.Empty;
            functionsCode.AppendLine(code);
        }

        return $@"/* ### GENERATED CODE - Do not modify. Edits will be lost on build. ### */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace {pluginNamespace};

public class {functionName}
{{
    private readonly ILogger _logger;
    private readonly IAiPluginRunner _pluginRunner;

    public {functionName}(ILoggerFactory loggerFactory, IAiPluginRunner pluginRunner)
    {{
        this._logger = loggerFactory.CreateLogger<{functionName}>();
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

        PromptTemplateConfig? promptTemplateConfig = JsonSerializer.Deserialize<PromptTemplateConfig>(metadataJson!);

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