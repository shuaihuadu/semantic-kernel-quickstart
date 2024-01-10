﻿namespace KernelSyntaxExamples.RepoUtils;

internal static class ConsoleLogger
{
    private static readonly Lazy<ILoggerFactory> _loggerFactory = new(LogBuilder);

    internal static ILoggerFactory LoggerFactory => _loggerFactory.Value;

    internal static ILogger Logger => LoggerFactory.CreateLogger<object>();

    private static ILoggerFactory LogBuilder()
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);

            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);

            builder.AddConsole();
        });
    }
}
