namespace HelloAgent.Plugins;

public sealed class EmailPlugin
{
    [KernelFunction]
    [Description("Sends an email to a recipient.")]
    public void SendEmailAsync(Kernel kernel,
        [Description("Semicolon delimitated list of emails of the recipients")]
        string recipientEmails,
        string subjcet,
        string body)
    {
        Console.WriteLine($"Email sent(use {nameof(EmailPlugin)})!");
    }
}
