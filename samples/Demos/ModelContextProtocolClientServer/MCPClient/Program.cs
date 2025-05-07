// Copyright (c) IdeaTech. All rights reserved.


using MCPClient.Samples;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        //await MCPToolsSample.RunAsync();

        //await MCPPromptSample.RunAsync();

        //await MCPResourcesSample.RunAsync();

        //await MCPResourceTemplatesSample.RunAsync();

        //await MCPSamplingSample.RunAsync();

        //await ChatCompletionAgentWithMCPToolsSample.RunAsync();

        //await AzureAIAgentWithMCPToolsSample.RunAsync();

        await AgentAvailableAsMCPToolSample.RunAsync();
    }
}
