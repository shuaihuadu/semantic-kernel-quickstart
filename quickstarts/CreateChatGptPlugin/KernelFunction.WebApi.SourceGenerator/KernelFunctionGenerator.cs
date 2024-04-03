﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KernelFunction.WebApi.SourceGenerator
{
    [Generator]
    public class KernelFunctionGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            Dictionary<string, List<FunctionDetail>> functionDetailsByPlugin = new Dictionary<string, List<FunctionDetail>>();

            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

                SyntaxNode? root = syntaxTree.GetRoot();

                IEnumerable<InvocationExpressionSyntax> configureServicesCalls = root.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(ies => ies.Expression is MemberAccessExpressionSyntax maes && maes.Name.ToString() == "AddTransient");

                foreach (var configureServicesCall in configureServicesCalls)
                {
                    foreach (var invocation in configureServicesCall.DescendantNodes().OfType<InvocationExpressionSyntax>())
                    {
                        IMethodSymbol? symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                        if (symbol?.ContainingType.ToString() == "Microsoft.SemanticKernel.KernelExtensions")
                        {
                            INamedTypeSymbol? pluginTypeArgument = null;

                            if (symbol.Name == "AddFromType")
                            {
                                pluginTypeArgument = symbol.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
                            }
                            else if (symbol.Name == "AddFromObject")
                            {
                                ObjectCreationExpressionSyntax? objectCreationExpression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression as ObjectCreationExpressionSyntax;

                                if (objectCreationExpression != null)
                                {
                                    TypeInfo typeInfo = semanticModel.GetTypeInfo(objectCreationExpression);

                                    pluginTypeArgument = typeInfo.Type as INamedTypeSymbol;
                                }
                            }

                            if (pluginTypeArgument != null && configureServicesCall.Expression is MemberAccessExpressionSyntax maes)
                            {
                                string pluginName = pluginTypeArgument.Name;

                                List<FunctionDetail> functionDetails = this.ExtractFunctionDetails(context, pluginTypeArgument);

                                functionDetailsByPlugin[pluginName] = functionDetails;
                            }
                        }
                    }
                }
            }

            foreach (var pluginEntry in functionDetailsByPlugin)
            {
                string sourceCode = GenerateClassCode("AzureFunctionPlugins", pluginEntry.Key, pluginEntry.Value);

                context.AddSource($"{pluginEntry.Key}.g.cs", sourceCode);
            }
        }

        private string GenerateClassCode(string rootNamespace, string pluginName, List<FunctionDetail> functions)
        {
            StringBuilder functionsCode = new();

            foreach (FunctionDetail function in functions)
            {
                functionsCode.AppendLine(GenerateFunctionSource(pluginName, function) ?? string.Empty);
            }

            return $@"/* ### GENERATED CODE - Do not modify. Edits will be lost on build. ### */
        using System;
        using System.Net;
        using System.Reflection;
        using System.Threading.Tasks;
        using Microsoft.Azure.Functions.Worker;
        using Microsoft.Azure.Functions.Worker.Http;
        using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
        using Microsoft.Extensions.Logging;
        using Microsoft.OpenApi.Models;
        using Microsoft.SemanticKernel;
        using Plugins.AzureFunctions.Extensions;

        namespace {rootNamespace};

        public class {pluginName}
        {{
            private readonly ILogger _logger;
            private readonly AIPluginRunner _pluginRunner;

            public {pluginName}(AIPluginRunner pluginRunner, ILoggerFactory loggerFactory)
            {{
                _pluginRunner = pluginRunner;
                _logger = loggerFactory.CreateLogger<{pluginName}>();
            }}

            {functionsCode}
        }}";
        }

        private string? GenerateFunctionSource(string pluginName, FunctionDetail functionDetail)
        {
            string modelClassName = $"{functionDetail.Name}Model";

            string parameterAttributes = GenerateModelClassSource(modelClassName, functionDetail.Parameters);

            return $@"
            {parameterAttributes}

            [OpenApiOperation(operationId: ""{functionDetail.Name}"", tags: new[] {{ ""{functionDetail.Name}"" }})]
            [OpenApiRequestBody(contentType: ""application/json"", bodyType: typeof({modelClassName}), Required = true, Description = ""JSON request body"")]
            [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ""application/json"", bodyType: typeof(string), Description = ""The OK response"")]
            [Function(""{functionDetail.Name}"")]
            public async Task<HttpResponseData> {functionDetail.Name}([HttpTrigger(AuthorizationLevel.Anonymous, ""post"")] HttpRequestData req)
            {{
                _logger.LogInformation(""HTTP trigger processed a request for function {pluginName}-{functionDetail.Name}."");
                return await _pluginRunner.RunAIPluginOperationAsync<{modelClassName}>(req, ""{pluginName}"", ""{functionDetail.Name}"");
            }}";
        }

        private string GenerateModelClassSource(string modelClassName, List<ParameterDatail> parameters)
        {
            StringBuilder modelClassBuilder = new();

            modelClassBuilder.AppendLine($"public class {modelClassName}");
            modelClassBuilder.AppendLine("{");

            foreach (ParameterDatail parameter in parameters)
            {
                modelClassBuilder.AppendLine($"    public {parameter.Type} {parameter.Name} {{ get;set; }}");
            }

            modelClassBuilder.AppendLine("}");

            return modelClassBuilder.ToString();
        }

        private List<FunctionDetail> ExtractFunctionDetails(GeneratorExecutionContext context, INamedTypeSymbol pluginClass)
        {
            List<FunctionDetail> functionDetails = new List<FunctionDetail>();

            foreach (var member in pluginClass.GetMembers())
            {
                if (member is IMethodSymbol methodSymbol && methodSymbol.GetAttributes().Any(attr => attr?.AttributeClass?.Name == "KernelFunctionAttribute"))
                {
                    FunctionDetail functionDetail = new FunctionDetail
                    {
                        Name = methodSymbol.Name,
                        Description = methodSymbol.GetAttributes().FirstOrDefault(a => a?.AttributeClass?.Name == "DescriptionAttribute")?.ConstructorArguments.FirstOrDefault().Value.ToString(),
                        Parameters = new List<ParameterDatail>()
                    };

                    foreach (var parameter in methodSymbol.Parameters)
                    {
                        ParameterDatail parameterDatail = new ParameterDatail
                        {
                            Name = parameter.Name,
                            Type = parameter.Type.ToString(),
                            Description = parameter.GetAttributes().FirstOrDefault(a => a?.AttributeClass?.Name == "DescriptionAttribute")?.ConstructorArguments.FirstOrDefault().Value.ToString()
                        };

                        functionDetail.Parameters.Add(parameterDatail);
                    }

                    functionDetails.Add(functionDetail);
                }
            }

            return functionDetails;
        }
    }

    public class FunctionDetail
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<ParameterDatail> Parameters { get; set; } = new List<ParameterDatail>();
    }

    public class ParameterDatail
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}