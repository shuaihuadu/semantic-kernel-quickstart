namespace HomeAutomation.Plugins;

public class MyTimePlugin
{
    [KernelFunction, Description("Get the current time.")]
    public DateTimeOffset Time() => DateTimeOffset.UtcNow;
}
