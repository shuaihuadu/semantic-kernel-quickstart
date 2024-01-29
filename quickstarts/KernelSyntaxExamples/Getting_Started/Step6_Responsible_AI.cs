namespace KernelSyntaxExamples.GettingStart;

public class Step6_Responsible_AI : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey
            );

        builder.Services.AddSingleton(this.Output);

        builder.Services.AddSingleton<IPromptFilter, PromptFilter>();

        Kernel kernel = builder.Build();

        KernelArguments arguments = new()
        {
            { "card_number","4234 6536 6545 2373"}
        };

        FunctionResult result = await kernel.InvokePromptAsync("Tell me some useful information about this credit card number {{$card_number}}", arguments);

        WriteLine(result.ToString());
    }


    private sealed class PromptFilter : IPromptFilter
    {
        private ITestOutputHelper _output;

        public PromptFilter(ITestOutputHelper output)
        {
            this._output = output;
        }

        public void OnPromptRendered(PromptRenderedContext context)
        {
            context.RenderedPrompt += " NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY";

            this._output.WriteLine(context.RenderedPrompt);
        }

        public void OnPromptRendering(PromptRenderingContext context)
        {
            if (context.Arguments.ContainsName("card_number"))
            {
                context.Arguments["card_number"] = "**** **** **** ****";
            }
        }
    }

    public Step6_Responsible_AI(ITestOutputHelper output) : base(output)
    {
    }
}
