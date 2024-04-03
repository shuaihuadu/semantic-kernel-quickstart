using Microsoft.CodeAnalysis;

namespace AiPlugin.Functions.SourceGenerator.Extensions
{
    internal static class GeneratorExecutionContextExtensions
    {
        public static string? GetMSBuildProperty(
            this GeneratorExecutionContext context,
            string name,
            string defaultValue = "")
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var value);

            return value ?? defaultValue;
        }

        public static string? GetRootNameSpace(this GeneratorExecutionContext context)
        {
            return context.GetMSBuildProperty("RootNamespace");
        }
    }
}