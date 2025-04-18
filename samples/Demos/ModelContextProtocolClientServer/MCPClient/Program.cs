// Copyright (c) IdeaTech. All rights reserved.


using Azure.AI.Projects;
using Azure.Identity;
using MCPClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        //await UseMCPToolsAsync();

        //await UseMCPPromptAsync();

        //await UseMCPResourcesAsync();

        //await UseMCPResourceTemplateAsync();

        //await UseMCPSamplingAsync();

        //await UseChatCompletionAgentWithMCPToolsAsync();

        //await UseAzureAIAgentWithMCPToolsAsync();
    }

    private static async Task UseMCPToolsAsync()
    {
        Console.WriteLine($"Running the {nameof(UseMCPToolsAsync)} sample.");

        await using IMcpClient mcpClient = await CreateMcpClientAsync();

        IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

        DisplayTools(tools);

        Kernel kernel = CreateKernelWithChatCompletionService();
        kernel.Plugins.AddFromFunctions("Tools", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            Temperature = 0,
            ChatSystemPrompt = "请保持使用中文",
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
        };

        string prompt = "What is the likely color of the sky in Boston today?";
        Console.WriteLine(prompt);

        FunctionResult result = await kernel.InvokePromptAsync(prompt, new(executionSettings));

        Console.WriteLine(result);
        Console.WriteLine();
    }

    private static async Task UseMCPPromptAsync()
    {
        Console.WriteLine($"Running the {nameof(UseMCPPromptAsync)} samples.");

        await using IMcpClient mcpClient = await CreateMcpClientAsync();

        IList<McpClientPrompt> prompts = await mcpClient.ListPromptsAsync();

        DisplayPrompts(prompts);

        Kernel kernel = CreateKernelWithChatCompletionService();

        GetPromptResult bostonWeatherPrompt = await mcpClient.GetPromptAsync("GetCurrentWeatherForCity", new Dictionary<string, object?>() { ["city"] = "Boston", ["time"] = DateTime.UtcNow.ToString() });

        GetPromptResult sydneyWeatherPrompt = await mcpClient.GetPromptAsync("GetCurrentWeatherForCity", new Dictionary<string, object?>() { ["city"] = "Sydney", ["time"] = DateTime.UtcNow.ToString() });

        ChatHistory chatHistory = [];
        chatHistory.AddRange(bostonWeatherPrompt.ToChatMessageContents());
        chatHistory.AddRange(sydneyWeatherPrompt.ToChatMessageContents());
        chatHistory.AddUserMessage("Compare the weather in the two cities and suggest the best place to go for a walk. Reply in Chinese.");

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent result = await chatCompletion.GetChatMessageContentAsync(chatHistory, kernel: kernel);

        Console.WriteLine(result);
        Console.WriteLine();
    }

    private static async Task UseMCPResourcesAsync()
    {
        Console.WriteLine($"Running the {nameof(UseMCPResourcesAsync)} sample.");

        await using IMcpClient mcpClient = await CreateMcpClientAsync();

        IList<Resource> resources = await mcpClient.ListResourcesAsync();

        DisplayResources(resources);

        Kernel kernel = CreateKernelWithChatCompletionService();

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            ChatSystemPrompt = "请保持使用中文",
            Temperature = 0,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
        };

        ReadResourceResult resource = await mcpClient.ReadResourceAsync("image://cat.jpg");

        ChatHistory chatHisotry = [];
        chatHisotry.AddUserMessage(resource.ToChatMessageContentItemCollection());
        chatHisotry.AddUserMessage("Descirbe the content of the image?");

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent result = await chatCompletion.GetChatMessageContentAsync(chatHisotry, executionSettings, kernel);

        Console.WriteLine(result);
        Console.WriteLine();
    }

    private static async Task UseMCPResourceTemplateAsync()
    {
        Console.WriteLine($"Running the {nameof(UseMCPResourceTemplateAsync)} sample.");

        await using IMcpClient mcpClient = await CreateMcpClientAsync();

        IList<ResourceTemplate> resourceTemplates = await mcpClient.ListResourceTemplatesAsync();
        DisplayResourceTemplates(resourceTemplates);

        Kernel kernel = CreateKernelWithChatCompletionService();

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            ChatSystemPrompt = "请保持使用中文",
            Temperature = 0,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
        };

        string prompt = "What is the Semantic Kernel?";

        ReadResourceResult resource = await mcpClient.ReadResourceAsync($"vectorStore://records/{prompt}");

        ChatHistory chatHisotry = [];
        chatHisotry.AddUserMessage(resource.ToChatMessageContentItemCollection());
        chatHisotry.AddUserMessage(prompt);

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent result = await chatCompletion.GetChatMessageContentAsync(chatHisotry, executionSettings, kernel);

        Console.WriteLine(result);
        Console.WriteLine();
    }

    private static async Task UseMCPSamplingAsync()
    {
        Console.WriteLine($"Running the {nameof(UseMCPSamplingAsync)} sample.");

        Kernel kernel = CreateKernelWithChatCompletionService();

        kernel.FunctionInvocationFilters.Add(new HumanInTheLoopFilter());

        await using IMcpClient mcpClient = await CreateMcpClientAsync(kernel, SamplingRequestHandlerAsync);

        IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

        kernel.Plugins.AddFromFunctions("Tools", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            Temperature = 0,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
        };

        string prompt = "Create a schedule for me based on the latest unread emails in my inbox.";

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        ChatMessageContent result = await chatCompletion.GetChatMessageContentAsync(prompt, executionSettings, kernel);

        Console.WriteLine(result);
        Console.WriteLine();
    }

    private static async Task UseChatCompletionAgentWithMCPToolsAsync()
    {
        Console.WriteLine($"Running the {nameof(UseChatCompletionAgentWithMCPToolsAsync)} sample.");

        await using IMcpClient mcpClient = await CreateMcpClientAsync();

        IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

        DisplayTools(tools);

        Kernel kernel = CreateKernelWithChatCompletionService();
        kernel.Plugins.AddFromFunctions("Tools", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
        };

        string prompt = "What is the likely color of the sky in Boston today?";
        Console.WriteLine(prompt);

        ChatCompletionAgent agent = new()
        {
            Instructions = "Answer questions about the weather.",
            Name = "WeatherAgent",
            Kernel = kernel,
            Arguments = new KernelArguments(executionSettings)
        };

        ChatMessageContent response = await agent.InvokeAsync(prompt).FirstAsync();

        Console.WriteLine(response);
        Console.WriteLine();
    }

    private static async Task UseAzureAIAgentWithMCPToolsAsync()
    {
        Console.WriteLine($"Running the {nameof(UseAzureAIAgentWithMCPToolsAsync)}");

        await using IMcpClient mcpClient = await CreateMcpClientAsync();

        IList<McpClientTool> tools = await mcpClient.ListToolsAsync();
        DisplayTools(tools);

        Kernel kernel = new();
        kernel.Plugins.AddFromFunctions("Tools", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

        AzureAIAgent agent = await CreateAzureAIAgentAsync(name: "WeatherAgent", instructions: "Answer questions about the weather.", kernel: kernel);

        string prompt = "What is the likely color of the sky in Boston today?";
        Console.WriteLine(prompt);

        AgentResponseItem<ChatMessageContent> response = await agent.InvokeAsync(message: prompt).FirstAsync();
        Console.WriteLine(response.Message);
        Console.WriteLine();

        await response!.Thread.DeleteAsync();

        await agent.Client.DeleteAgentAsync(agent.Id);
    }

    private static async Task<CreateMessageResult> SamplingRequestHandlerAsync(Kernel kernel, CreateMessageRequestParams? request, IProgress<ProgressNotificationValue> progress, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        AzureOpenAIPromptExecutionSettings executionSettings = new()
        {
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            StopSequences = request.StopSequences?.ToList()
        };

        ChatHistory chatHistory = [];

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            chatHistory.AddSystemMessage(request.SystemPrompt);
        }
        chatHistory.AddRange(request.Messages.ToChatMessageContents());

        IChatCompletionService chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        ChatMessageContent result = await chatCompletion.GetChatMessageContentAsync(chatHistory, executionSettings, cancellationToken: cancellationToken);

        return result.ToCreateMessageResult();
    }

    private static Kernel CreateKernelWithChatCompletionService()
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

    private static async Task<AzureAIAgent> CreateAzureAIAgentAsync(Kernel kernel, string name, string instructions)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(@"D:\appsettings\semantic-kernel-quickstart.json", true)
            .Build();

        string chatModelId = config["AzureAI:ChatModelId"] ?? throw new ArgumentNullException("endpoint");

        string connectionString = config["AzureAI:ConnectionString"] ?? throw new ArgumentNullException("deploymentName");

        AIProjectClient projectClient = AzureAIAgent.CreateAzureAIClient(connectionString, new AzureCliCredential());

        AgentsClient agentsClient = projectClient.GetAgentsClient();

        Azure.AI.Projects.Agent agent = await agentsClient.CreateAgentAsync(chatModelId, name, null, instructions);

        return new AzureAIAgent(agent, agentsClient)
        {
            Kernel = kernel
        };
    }

    private static Task<IMcpClient> CreateMcpClientAsync(Kernel? kernel = null, Func<Kernel, CreateMessageRequestParams?, IProgress<ProgressNotificationValue>, CancellationToken, Task<CreateMessageResult>>? samplingRequestHandler = null)
    {
        KernelFunction? skSamplingHandler = null;

        return McpClientFactory.CreateAsync(
            clientTransport: new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "MCPServer",
                Command = GetMCPServerPath()
            }),
            clientOptions: samplingRequestHandler != null ? new McpClientOptions()
            {
                Capabilities = new ClientCapabilities
                {
                    Sampling = new SamplingCapability
                    {
                        SamplingHandler = InvokeHandlerAsync
                    }
                }
            } : null
        );

        async ValueTask<CreateMessageResult> InvokeHandlerAsync(CreateMessageRequestParams? request, IProgress<ProgressNotificationValue> progress, CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            skSamplingHandler ??= KernelFunctionFactory.CreateFromMethod((CreateMessageRequestParams? request, IProgress<ProgressNotificationValue> progress, CancellationToken ct) =>
            {
                return samplingRequestHandler(kernel!, request, progress, ct);
            }, "MCPSamplingHandler");

            KernelArguments kernelArguments = new()
            {
                ["request"] = request,
                ["progress"] = progress
            };

            FunctionResult functionResult = await skSamplingHandler.InvokeAsync(kernel!, kernelArguments, cancellationToken);

            return functionResult.GetValue<CreateMessageResult>()!;
        }
    }

    private static string GetMCPServerPath()
    {
        string configuration;

#if DEBUG
        configuration = "Debug";
#else
        configuration = "Release";
#endif
        return Path.Combine("..", "..", "..", "..", "MCPServer", "bin", configuration, "net9.0", "MCPServer.exe");
    }

    private static void DisplayTools(IList<McpClientTool> tools)
    {
        Console.WriteLine("Available MCP tools:");

        foreach (var tool in tools)
        {
            Console.WriteLine($"- Name: {tool.Name}, Description: {tool.Description}");
        }

        Console.WriteLine();
    }

    private static void DisplayPrompts(IList<McpClientPrompt> prompts)
    {
        Console.WriteLine("Available MCP prompts:");

        foreach (var prompt in prompts)
        {
            Console.WriteLine($"- Name: {prompt.Name}, Description: {prompt.Description}");
        }

        Console.WriteLine();
    }

    private static void DisplayResources(IList<Resource> resources)
    {
        Console.WriteLine("Available MCP resources:");

        foreach (Resource resource in resources)
        {
            Console.WriteLine($"- Name: {resource.Name}, Uri: {resource.Uri}, Description: {resource.Description}");
        }

        Console.WriteLine();
    }

    private static void DisplayResourceTemplates(IList<ResourceTemplate> resourceTemplates)
    {
        Console.WriteLine("Available MCP resource tenmplates:");

        foreach (var template in resourceTemplates)
        {
            Console.WriteLine($"- Name: {template.Name}, Description: {template.Description}");
        }

        Console.WriteLine();
    }
}
