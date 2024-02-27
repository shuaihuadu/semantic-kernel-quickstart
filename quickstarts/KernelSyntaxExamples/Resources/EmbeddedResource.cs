namespace KernelSyntaxExamples.Resources;

internal static class EmbeddedResource
{
    private static readonly string? resourceNamespace = typeof(EmbeddedResource).Namespace;

    internal static string Read(string fileName)
    {
        Assembly? assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly ?? throw new ConfigurationException($"[{resourceNamespace}] {fileName} assembly not found");

        string resourceName = $"{resourceNamespace}.{fileName}";

        using Stream? resource = assembly.GetManifestResourceStream(resourceName) ?? throw new ConfigurationException($"{resourceName} resource not found");

        using StreamReader reader = new(resource);

        return reader.ReadToEnd();
    }


    internal static Stream? ReadStream(string fileName)
    {
        Assembly? assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly ?? throw new ConfigurationException($"[{resourceNamespace}] {fileName} assembly not found");

        string resourceName = $"{resourceNamespace}.{fileName}";

        return assembly.GetManifestResourceStream(resourceName);
    }

    internal async static Task<ReadOnlyMemory<byte>> ReadAllAsync(string fileName)
    {
        await using Stream? resourceStream = ReadStream(fileName);
        using MemoryStream memoryStream = new();

        await resourceStream!.CopyToAsync(memoryStream);

        return new ReadOnlyMemory<byte>(memoryStream.ToArray());
    }
}
