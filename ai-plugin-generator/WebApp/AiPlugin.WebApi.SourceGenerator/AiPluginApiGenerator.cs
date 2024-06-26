﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AiPlugin.WebApi.SourceGenerator
{
    [Generator]
    public class AiPluginApiGenerator : ISourceGenerator
    {

        private const string DefaultFunctionNamespace = "AiPlugin.Web.Controllers";
        private const string FunctionConfigFileName = "config.json";
        private const string FunctionPromptFileName = "skprompt.txt";

        public void Execute(GeneratorExecutionContext context)
        {
            string rootNamespace = DefaultFunctionNamespace;
            //string? rootNamespace = context.GetRootNameSpace();

            //if (string.IsNullOrEmpty(rootNamespace))
            //{
            //    rootNamespace = DefaultFunctionNamespace;
            //}

            IEnumerable<AdditionalText> functionFiles = context.AdditionalFiles.Where(f =>
            f.Path.Contains(FunctionConfigFileName) ||
            f.Path.Contains(FunctionPromptFileName));

            //获取所有Build Action属性设置为C# analyzer additional file的文件，并按照其所在的目录进行分组
            IEnumerable<IGrouping<string, AdditionalText>> functionFileGroups = functionFiles.GroupBy(f => Path.GetDirectoryName(f.Path));

            //再按照具体的插件进行分组，获取Function所属的Plugin的目录，譬如：Plugins目录下面有APlugin、BPlugin，此处获取的是以APlugin和BPlugin为Key的分组结果
            IEnumerable<IGrouping<string, IGrouping<string, AdditionalText>>> pluginFolderGroups = functionFileGroups.GroupBy(f => Path.GetFileName(Path.GetDirectoryName(f.Key)));

            foreach (var pluginFolder in pluginFolderGroups)
            {
                string pluginName = pluginFolder.Key!;

                string className = $"{pluginName}Controller";

                string classSource = GenerateClassSource(rootNamespace!, className, pluginName, pluginFolder!);

                context.AddSource(className, SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string GenerateClassSource(string rootNamespace, string className, string pluginName, IGrouping<string, IGrouping<string, AdditionalText>> functionGroups)
        {
            StringBuilder functionsCode = new();

            foreach (var functionGroup in functionGroups)
            {

                string functionName = Path.GetFileName(functionGroups.Key);

                AdditionalText? promptFile = functionGroup.FirstOrDefault(f => Path.GetFileName(f.Path).Equals(FunctionPromptFileName, StringComparison.InvariantCultureIgnoreCase));
                AdditionalText? configFile = functionGroup.FirstOrDefault(f => Path.GetFileName(f.Path).Equals(FunctionConfigFileName, StringComparison.InvariantCultureIgnoreCase));

                if (promptFile != default && configFile != default)
                {
                    string code = GenerateFunctionSource(pluginName, promptFile, configFile) ?? string.Empty;
                    functionsCode.AppendLine(code);
                }
            }

            return $@"// <auto-generated />
using Microsoft.AspNetCore.Mvc;

namespace {rootNamespace};

[ApiController]
[Route(""api/plugins/[controller]"")]
public class {className} : ControllerBase
{{
    private readonly ILogger _logger;
    private readonly IAiPluginRunner _pluginRunner;

    public {className}(ILoggerFactory loggerFactory, IAiPluginRunner pluginRunner)
    {{
        this._logger = loggerFactory.CreateLogger<{className}>();
        this._pluginRunner = pluginRunner;
    }}

    {functionsCode}
}}";
        }

        private static string? GenerateFunctionSource(string pluginName, AdditionalText promptFile, AdditionalText configFile)
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

            PromptTemplateConfig? promptTemplateConfig = JsonConvert.DeserializeObject<PromptTemplateConfig>(metadataJson!);

            if (promptTemplateConfig is null) { return null; }

            string descriptionProperty = string.IsNullOrWhiteSpace(promptTemplateConfig.Description) ? string.Empty : promptTemplateConfig.Description!;

            string parameters = GenerateParameterSource(promptTemplateConfig.InputVariables);

            string parameterComments = GenerateParameterComments(promptTemplateConfig.InputVariables);

            return $@"
    /// <summary>
    /// {descriptionProperty}
    /// </summary>
    /// <returns>The plugin execute result.</returns>{parameterComments}    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [HttpPost(""{functionName}"")]
    public async Task<string> {functionName}({parameters})
    {{
        this._logger.LogInformation(""HTTP a request for plugin [{pluginName}] - function [{functionName}]."");

        return await this._pluginRunner.RunAiPluginOperationAsync(Request,""{pluginName}"",""{functionName}"");
    }}";
        }

        private static string GenerateParameterSource(List<InputVariable> inputVariables)
        {
            StringBuilder parameterStringBuilder = new();

            if (inputVariables != null)
            {
                foreach (InputVariable inputVariable in inputVariables)
                {
                    parameterStringBuilder.Append($@"[FromQuery] string {inputVariable.Name},");
                }
            }

            return parameterStringBuilder.ToString().TrimEnd(',');
        }

        private static string GenerateParameterComments(List<InputVariable> inputVariables)
        {
            StringBuilder parameterCommentsStringBuilder = new();

            parameterCommentsStringBuilder.AppendLine();

            if (inputVariables != null)
            {
                foreach (var inputVariable in inputVariables)
                {
                    parameterCommentsStringBuilder.AppendLine($@"    /// <param name=""{inputVariable.Name}"">{inputVariable.Description}</param>");
                }
            }

            return parameterCommentsStringBuilder.ToString();
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
