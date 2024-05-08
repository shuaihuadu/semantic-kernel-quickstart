﻿// Copyright (c) Microsoft. All rights reserved.

namespace Resources.Plugins;

public sealed class MenuPlugin
{
    public const string CorrelationIdArgument = "correlationId";

    private readonly List<string> _correlationIds = [];

    public IReadOnlyList<string> CorrelationIds => _correlationIds;

    [KernelFunction, Description("Provides a list of specials from the menu.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Too smart")]
    public string GetSpecials()
    {
        return @"
Special Soup: Clam Chowder
Special Salad: Cobb Salad
Special Drink: Chai Tea
";
    }

    [KernelFunction, Description("Provides the price of the requested menu item.")]
    public string GetItemPrice(
        [Description("The name of the menu item.")]
            string menuItem)
    {
        return "$9.99";
    }
}
