namespace HomeAutomation.Plugins;

public class MyAlarmPlugin(MyTimePlugin timePlugin)
{
    private readonly MyTimePlugin _timePlugin = timePlugin;

    [KernelFunction, Description("Sets an alarm at the provided time.")]
    public void SetAlarm(string alarm)
    {
        Console.WriteLine("Set alarm in MyAlarmPlugin");
    }
}
