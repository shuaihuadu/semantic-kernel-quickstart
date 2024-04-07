namespace DocumentationExamples;

public abstract class BaseTest
{
    protected ITestOutputHelper Output { get; }

    protected List<string> SimulatedInputText = [];

    protected int SimulatedInputTextIndex = 0;

    protected BaseTest(ITestOutputHelper output)
    {
        this.Output = output;

        TestConfiguration.Initialize();
    }

    protected void WriteLine(object? target = null)
    {
        this.Output.WriteLine(target?.ToString() ?? string.Empty);
    }

    protected void Write(object? target = null)
    {
        this.Output.WriteLine(target?.ToString() ?? string.Empty);
    }

    protected string? ReadLine()
    {
        if (SimulatedInputTextIndex < SimulatedInputText.Count)
        {
            return SimulatedInputText[SimulatedInputTextIndex++];
        }

        return null;
    }
}