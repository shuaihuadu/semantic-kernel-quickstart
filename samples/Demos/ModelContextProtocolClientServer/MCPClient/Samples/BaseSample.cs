// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;

namespace MCPClient.Samples;

internal abstract class BaseSample
{
    /// <summary>
    /// Creates an MCP client and connects it to the MCPServer server.
    /// </summary>
    /// <param name="kernel">Optional kernel instance to use for the MCP client.</param>
    /// <param name="samplingRequestHandler">Optional handler for MCP sampling requests.</param>
    /// <returns>An instance of <see cref="IMcpClient"/>.</returns>
    protected static Task<IMcpClient> CreateMcpClientAsync(
        Kernel? kernel = null,
        Func<Kernel, CreateMessageRequestParams?, IProgress<ProgressNotificationValue>, CancellationToken, Task<CreateMessageResult>>? samplingRequestHandler = null)
    {
        KernelFunction? skSamplingHandler = null;

        // Create and return the MCP client
        return McpClientFactory.CreateAsync(
            clientTransport: new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "MCPServer",
                Command = GetMCPServerPath(), // Path to the MCPServer executable
            }),
            clientOptions: samplingRequestHandler != null ? new McpClientOptions()
            {
                Capabilities = new ClientCapabilities
                {
                    Sampling = new SamplingCapability
                    {
                        SamplingHandler = InvokeHandlerAsync
                    },
                },
            } : null
         );

        async ValueTask<CreateMessageResult> InvokeHandlerAsync(CreateMessageRequestParams? request, IProgress<ProgressNotificationValue> progress, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            skSamplingHandler ??= KernelFunctionFactory.CreateFromMethod(
                (CreateMessageRequestParams? request, IProgress<ProgressNotificationValue> progress, CancellationToken ct) =>
                {
                    return samplingRequestHandler(kernel!, request, progress, ct);
                },
                "MCPSamplingHandler"
            );

            // The argument names must match the parameter names of the delegate the SK Function is created from
            KernelArguments kernelArguments = new()
            {
                ["request"] = request,
                ["progress"] = progress
            };

            FunctionResult functionResult = await skSamplingHandler.InvokeAsync(kernel!, kernelArguments, cancellationToken);

            return functionResult.GetValue<CreateMessageResult>()!;
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="Kernel"/> with the Azure OpenAI chat completion service registered.
    /// </summary>
    /// <returns>An instance of <see cref="Kernel"/>.</returns>
    protected static Kernel CreateKernelWithChatCompletionService()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
            .Build();

        string endpoint = config["AzureOpenAI:Endpoint"] ?? throw new ArgumentNullException("endpoint");

        string deploymentName = config["AzureOpenAI:DeploymentName"] ?? throw new ArgumentNullException("deploymentName");

        string apiKey = config["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("apiKey");

        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.Services.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

        return kernelBuilder.Build();
    }

    /// <summary>
    /// Displays the list of available MCP tools.
    /// </summary>
    /// <param name="tools">The list of the tools to display.</param>
    protected static void DisplayTools(IList<McpClientTool> tools)
    {
        Console.WriteLine("Available MCP tools:");
        foreach (var tool in tools)
        {
            Console.WriteLine($"- Name: {tool.Name}, Description: {tool.Description}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Returns the path to the MCPServer server executable.
    /// </summary>
    /// <returns>The path to the MCPServer server executable.</returns>
    private static string GetMCPServerPath()
    {
        // Determine the configuration (Debug or Release)  
        string configuration;

#if DEBUG
        configuration = "Debug";
#else
        configuration = "Release";
#endif

        return Path.Combine("..", "..", "..", "..", "MCPServer", "bin", configuration, "net9.0", "MCPServer.exe");
    }
}
