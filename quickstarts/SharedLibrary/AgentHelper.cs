namespace SharedLibrary;

public class AgentHelper
{
    public static AgentBuilder CreareAgentBuilder()
    {
        TestConfiguration.Initialize();

        return new AgentBuilder()
            .WithAzureOpenAIChatCompletion(TestConfiguration.AzureOpenAI.Endpoint, TestConfiguration.AzureOpenAI.DeploymentName, TestConfiguration.AzureOpenAI.ApiKey);
    }
}
