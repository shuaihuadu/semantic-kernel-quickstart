
namespace KernelSyntaxExamples.GettingStart;

public class Step6_Responsible_AI : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddSingleton(this.Output);

        builder.Services.AddSingleton<IPromptRenderFilter, PromptFilter>();

        Kernel kernel = builder.Build();

        KernelArguments arguments = new()
        {
            { "card_number","4234 6536 6545 2373"}
        };

        FunctionResult result = await kernel.InvokePromptAsync("Tell me some useful information about this credit card number {{$card_number}}", arguments);

        WriteLine(result.ToString());
    }


    private sealed class PromptFilter : IPromptRenderFilter
    {
        private ITestOutputHelper _output;

        public PromptFilter(ITestOutputHelper output)
        {
            this._output = output;
        }

        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            if (context.Arguments.ContainsName("card_number"))
            {
                context.Arguments["card_number"] = "**** **** **** ****";
            }

            await next(context);

            context.RenderedPrompt += " NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY";
        }

    }

    public Step6_Responsible_AI(ITestOutputHelper output) : base(output)
    {
    }
}
