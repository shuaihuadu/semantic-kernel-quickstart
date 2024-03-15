namespace KernelSyntaxExamples.Plugins;

internal sealed class MenuPlugin
{
    [KernelFunction, Description("Providers a list of specials from the menu.")]
    public string SendEmail(
        [Description("The body of the email message to send.")] string input,
        [Description("The email address to send email to.")] string email_address)
        => $"Send email to: {email_address}. Body: {input}";

    [KernelFunction, Description("Given a name, find email address")]
    public string GetEmailAddress(
        [Description("The name of the person whose email address needs to be found.")] string input,
        ILogger? logger = null)
    {
        logger?.LogTrace("Returning hard coded email for {0}", input);

        return input switch
        {
            "Zhangsan" => "zhangsan@email.com",
            "Lisi" => "lisi@email.com",
            "Wangwu" => "wangwu@email.com",
            _ => "test@email.com"
        };
    }
}
