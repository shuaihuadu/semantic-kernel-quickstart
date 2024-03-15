﻿
namespace KernelSyntaxExamples;

public class Example73_AgentAuthoring(ITestOutputHelper output) : BaseTest(output)
{
    private static readonly List<IAgent> agents = [];

    [Fact(Skip = "Microsoft.SemanticKernel.HttpOperationException : Response status code does not indicate success: 400 (Bad Request).")]
    public async Task RunAgentAsync()
    {
        WriteLine("======== Example73_AgentAuthoring ========");

        try
        {
            IAgent articleGenerator = await CreateArticleGeneratorAsync();

            await foreach (IChatMessage message in articleGenerator.InvokeAsync("Chinese food is the best in the world"))
            {
                WriteLine($"[{message.Id}]");
                WriteLine($"# {message.Role}: {message.Content}");
            }
        }
        finally
        {
            await Task.WhenAll(agents.Select(a => a.DeleteAsync()));
        }
    }

    [Fact(Skip = "Microsoft.SemanticKernel.HttpOperationException : Response status code does not indicate success: 400 (Bad Request).")]
    public async Task RunAsPluginAsync()
    {
        WriteLine("======== Example73_AgentAuthoring ========");

        try
        {
            IAgent articleGenerator = await CreateArticleGeneratorAsync();

            string response = await articleGenerator.AsPlugin().InvokeAsync("Chinese food is the best in the world");

            WriteLine(response);
        }
        finally
        {
            await Task.WhenAll(agents.Select(a => a.DeleteAsync()));
        }
    }

    private async Task<IAgent> CreateArticleGeneratorAsync()
    {
        IAgent outlineGenerator = await CreateOutlineGeneratorAsync();
        IAgent sectionGenerator = await CreateResearchGeneratorAsync();

        return Track(
            await new AgentBuilder()
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ChatDeploymentName, TestConfiguration.AzureOpenAI.ApiKey)
            .WithInstructions("You write concise opinionated articles that are published online.  Use an outline to generate an article with one section of prose for each top-level outline element.  Each section is based on research with a maximum of 120 words.")
            .WithName("Article Author")
            .WithDescription("Author an article on a given topic.")
            .WithPlugin(outlineGenerator.AsPlugin())
            .WithPlugin(sectionGenerator.AsPlugin())
            .BuildAsync());

    }

    private static async Task<IAgent> CreateOutlineGeneratorAsync()
    {
        return Track(

            await new AgentBuilder()
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ChatDeploymentName, TestConfiguration.AzureOpenAI.ApiKey)
            .WithInstructions("Produce an single-level outline (no child elements) based on the given topic with at most 3 sections.")
            .WithName("Outline Generator")
            .WithDescription("Generate an outline")
            .BuildAsync());
    }

    private static async Task<IAgent> CreateResearchGeneratorAsync()
    {
        return Track(
            await new AgentBuilder()
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.ChatDeploymentName, TestConfiguration.AzureOpenAI.ApiKey)
            .WithInstructions("Provide insightful research that supports the given topic based on your knowledge of the outline topic.")
            .WithName("Researcher")
            .WithDescription("Author research summary.")
            .BuildAsync());
    }

    private static IAgent Track(IAgent agent)
    {
        agents.Add(agent);

        return agent;
    }
}
