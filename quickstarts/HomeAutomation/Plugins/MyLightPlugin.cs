namespace HomeAutomation.Plugins;

[Description("Represents a light")]
public class MyLightPlugin
{
    private bool _turnedOn;

    public MyLightPlugin(bool turnOn = false)
    {
        this._turnedOn = turnOn;
    }

    [KernelFunction, Description("Returns whether this light is on")]
    public bool IsTurnedOn() => this._turnedOn;

    [KernelFunction, Description("Turn on this light")]
    public void TurnOn() => this._turnedOn = true;

    [KernelFunction, Description("Turn off this light")]
    public void TurnOff() => this._turnedOn = false;
}