namespace KernelSyntaxExamples.RepoUtils;

public static class ObjectExtensions
{
    private static readonly JsonSerializerOptions jsonOptionsCache = new() { WriteIndented = true };

    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, jsonOptionsCache);
    }
}