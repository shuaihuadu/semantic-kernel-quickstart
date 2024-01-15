namespace KernelSyntaxExamples.DictionaryPlugin;

public sealed class ComplexParamsDictionaryPlugin
{
    public const string PluginName = nameof(ComplexParamsDictionaryPlugin);

    private readonly List<DictionaryEntry> _dictionary = new()
    {
        new DictionaryEntry("apple", "a round fruit with red, green, or yellow skin and a white flesh"),
        new DictionaryEntry("book", "a set of printed or written pages bound together along one edge"),
        new DictionaryEntry("cat", "a small furry animal with whiskers and a long tail that is often kept as a pet"),
        new DictionaryEntry("dog", "a domesticated animal with four legs, a tail, and a keen sense of smell that is often used for hunting or companionship"),
        new DictionaryEntry("elephant", "a large gray mammal with a long trunk, tusks, and ears that lives in Africa and Asia")
    };

    [KernelFunction, Description("Gets a random word from a dictionary of common words and their definitions.")]
    public DictionaryEntry GetRandomEnry()
    {
        int index = RandomNumberGenerator.GetInt32(0, this._dictionary.Count - 1);

        return this._dictionary[index];
    }

    [KernelFunction, Description("Gets the word for a given dictionary entry.")]
    public string GetWord([Description("Word to get definition for.")] DictionaryEntry entry)
    {
        return this._dictionary.FirstOrDefault(e => e.Word == entry.Word)?.Word ?? "Entry not found";
    }

    [KernelFunction, Description("Gets the definition for a given word.")]
    public string GetDefinition([Description("Word to get definition for.")] string word)
    {
        return this._dictionary.FirstOrDefault(e => e.Word == word)?.Definition ?? "Word not found";
    }
}

[TypeConverter(typeof(DictionaryConverter))]
public sealed class DictionaryEntry
{
    public string Word { get; set; } = string.Empty;

    public string Definition { get; set; } = string.Empty;

    public DictionaryEntry(string word, string definition)
    {
        this.Word = word;
        this.Definition = definition;
    }
}

public sealed class DictionaryConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => true;

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return JsonSerializer.Deserialize<DictionaryEntry>((string)value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return JsonSerializer.Serialize(value);
    }
}