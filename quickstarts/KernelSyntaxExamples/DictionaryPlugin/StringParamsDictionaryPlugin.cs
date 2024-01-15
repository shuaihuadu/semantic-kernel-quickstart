namespace KernelSyntaxExamples.DictionaryPlugin;

public sealed class StringParamsDictionaryPlugin
{
    public const string PluginName = nameof(StringParamsDictionaryPlugin);

    private readonly Dictionary<string, string> _dictionary = new()
    {
        {"apple", "a round fruit with red, green, or yellow skin and a white flesh"},
        {"book", "a set of printed or written pages bound together along one edge"},
        {"cat", "a small furry animal with whiskers and a long tail that is often kept as a pet"},
        {"dog", "a domesticated animal with four legs, a tail, and a keen sense of smell that is often used for hunting or companionship"},
        {"elephant", "a large gray mammal with a long trunk, tusks, and ears that lives in Africa and Asia"}
    };

    [KernelFunction, Description("Gets a random word fro a dictionary of common words and their definitions.")]
    public string GetRandomWord()
    {
        int index = RandomNumberGenerator.GetInt32(0, this._dictionary.Count - 1);

        return this._dictionary.ElementAt(index).Key;
    }

    [KernelFunction, Description("Gets the definition for a given word.")]
    public string GetDefinition([Description("Word to get definition for.")] string word)
    {
        return this._dictionary.TryGetValue(word, out string? definition) ? definition : "Word not found";
    }
}
