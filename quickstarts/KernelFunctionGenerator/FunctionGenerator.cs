namespace KernelFunctionGenerator;

public class FunctionGenerator : ISourceGenerator
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
        throw new NotImplementedException();
    }

    public void Initialize(GeneratorInitializationContext context) { }
}
