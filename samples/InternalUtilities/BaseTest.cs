﻿// Copyright (c) Microsoft. All rights reserved.
public abstract class BaseTest
{
    /// <summary>
    /// Flag to force usage of OpenAI configuration if both <see cref="TestConfiguration.OpenAI"/>
    /// and <see cref="TestConfiguration.AzureOpenAI"/> are defined.
    /// If 'false', Azure takes precedence.
    /// </summary>
    protected virtual bool ForceOpenAI { get; } = false;

    protected ITestOutputHelper Output { get; }

    protected ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// This property makes the samples Console friendly. Allowing them to be copied and pasted into a Console app, with minimal changes.
    /// </summary>
    public BaseTest Console => this;

    protected bool UseOpenAIConfig => this.ForceOpenAI || string.IsNullOrEmpty(TestConfiguration.AzureOpenAI.Endpoint);

    protected string ApiKey =>
        this.UseOpenAIConfig ?
            TestConfiguration.OpenAI.ApiKey :
            TestConfiguration.AzureOpenAI.ApiKey;

    protected string? Endpoint => UseOpenAIConfig ? null : TestConfiguration.AzureOpenAI.Endpoint;

    protected string Model =>
        this.UseOpenAIConfig ?
            TestConfiguration.OpenAI.ChatModelId :
            TestConfiguration.AzureOpenAI.DeploymentName;

    protected Kernel CreateKernelWithChatCompletion()
    {
        var builder = Kernel.CreateBuilder();

        if (this.UseOpenAIConfig)
        {
            builder.AddOpenAIChatCompletion(
                TestConfiguration.OpenAI.ChatModelId,
                TestConfiguration.OpenAI.ApiKey);
        }
        else
        {
            builder.AddAzureOpenAIChatCompletion(
                TestConfiguration.AzureOpenAI.DeploymentName,
                TestConfiguration.AzureOpenAI.Endpoint,
                TestConfiguration.AzureOpenAI.ApiKey);
        }

        return builder.Build();
    }

    protected BaseTest(ITestOutputHelper output)
    {
        this.Output = output;
        this.LoggerFactory = new XunitLogger(output);

        TestConfiguration.Initialize();
    }

    /// <summary>
    /// This method can be substituted by Console.WriteLine when used in Console apps.
    /// </summary>
    /// <param name="target">Target object to write</param>
    public void WriteLine(object? target = null)
    {
        this.Output.WriteLine(target ?? string.Empty);
    }

    /// <summary>
    /// This method can be substituted by Console.WriteLine when used in Console apps.
    /// </summary>
    /// <param name="format">Format string</param>
    /// <param name="args">Arguments</param>
    public void WriteLine(string? format, params object?[] args) => this.Output.WriteLine(format);// ?? string.Empty, args);

    /// <summary>
    /// Current interface ITestOutputHelper does not have a Write method. This extension method adds it to make it analogous to Console.Write when used in Console apps.
    /// </summary>
    /// <param name="target">Target object to write</param>
    public void Write(object? target = null)
    {
        this.Output.WriteLine(target ?? string.Empty);
    }
}
