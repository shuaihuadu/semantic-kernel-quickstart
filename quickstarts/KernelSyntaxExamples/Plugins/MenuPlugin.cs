namespace KernelSyntaxExamples.Plugins;

internal sealed class MenuPlugin
{
    [KernelFunction, Description("Providers a list of specials from the menu.")]
    public string GetSpecials()
    {
        return @"
Special Soup: Clam Chowder
Special Salad: Cobb Salad
Special Drink: Chai Tea
";
    }

    [KernelFunction, Description("Provider the price of the requested menu item.")]
    public string GetItemPrice([Description("The name of the menu item.")] string menuItem)
    {
        return "$9.99";
    }
}
