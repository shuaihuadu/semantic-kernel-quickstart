
namespace KernelSyntaxExamples;

public class Example65_HandlebarsPlanner : BaseTest
{
    private static int sampleIndex;
    private const string CourseraPluginName = "CourseraPlugin";

    private void WriteSampleHeadingToConsole(string name)
    {
        this.WriteLine($"======== [Handlebars Planner] Sample {sampleIndex++} -  Create and Execute Plan with: {name} ========");
    }

    private async Task RunSampleAsync(string goal, bool shouldPrintPrompt = false, params string[] pluginDirectoryNames)
    {
        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (deploymentName == null
            || endpoint == null
            || apiKey == null)
        {
            this.WriteLine("AzureOpenAI endpoint, apiKey, or deploymentName not found. Skipping example.");
            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: deploymentName,
                endpoint: endpoint,
                apiKey: apiKey,
                serviceId: "aoai")
            .Build();

        if (pluginDirectoryNames.Length > 0)
        {
            if (pluginDirectoryNames[0] == StringParamsDictionaryPlugin.PluginName)
            {
                kernel.ImportPluginFromType<StringParamsDictionaryPlugin>(StringParamsDictionaryPlugin.PluginName);
            }
            else if (pluginDirectoryNames[0] == ComplexParamsDictionaryPlugin.PluginName)
            {
                kernel.ImportPluginFromType<ComplexParamsDictionaryPlugin>(ComplexParamsDictionaryPlugin.PluginName);
            }
            else if (pluginDirectoryNames[0] == CourseraPluginName)
            {
                await kernel.ImportPluginFromOpenApiAsync(CourseraPluginName,
                    new Uri("https://www.coursera.org/api/rest/v1/search/openapi.yaml"));
            }
            else
            {
                string folder = RepoFiles.SamplePluginsPath();

                foreach (var pluginDirectoryName in pluginDirectoryNames)
                {
                    kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, pluginDirectoryName));
                }
            }
        }

        bool allowLoopsInPlan = deploymentName.Contains("gpt-4", StringComparison.OrdinalIgnoreCase);

        HandlebarsPlanner planner = new(new HandlebarsPlannerOptions
        {
            AllowLoops = allowLoopsInPlan
        });

        this.WriteLine($"Goal: {goal}");

        HandlebarsPlan plan = await planner.CreatePlanAsync(kernel, goal);

        if (shouldPrintPrompt && plan.Prompt is not null)
        {
            this.WriteLine($"\nPrompt template:\n{plan.Prompt}");
        }

        string result = await plan.InvokeAsync(kernel);
        this.WriteLine($"\nResult:\n{result}\n");
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public async Task PlanNotPossibleSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Plan Not Possible");

        try
        {
            await RunSampleAsync("Send Mary an email with the list of meetings I have scheduled today.", shouldPrintPrompt, "SummarizePlugin");
        }
        catch (KernelException ex) when (
            ex.Message.Contains(nameof(HandlebarsPlannerErrorCodes.InsufficientFunctionsForGoal), StringComparison.CurrentCultureIgnoreCase)
            || ex.Message.Contains(nameof(HandlebarsPlannerErrorCodes.HallucinatedHelpers), StringComparison.CurrentCultureIgnoreCase)
            || ex.Message.Contains(nameof(HandlebarsPlannerErrorCodes.InvalidTemplate), StringComparison.CurrentCultureIgnoreCase))
        {

            this.WriteLine($"\n${ex.Message}\n");
        }
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(true)]
    public async Task RunCourseraSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Coursera OpenAI Plugin");

        await RunSampleAsync("Show me courses about Artificial Intelligence.", shouldPrintPrompt, CourseraPluginName);
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public async Task RunDictionaryWithBasicTypesSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Basic Type using Local Dictionary Plugin");

        await RunSampleAsync("Get a random word and its definition.", shouldPrintPrompt, StringParamsDictionaryPlugin.PluginName);
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(true)]
    public async Task RunLocalDictionaryWithComplexTypesSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Complex Type using Local Dictionary Plugin");

        await RunSampleAsync("Teach me two random words and their definition.", shouldPrintPrompt, ComplexParamsDictionaryPlugin.PluginName);
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public async Task RunPoetrySampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Multiple Plugins");

        await RunSampleAsync("Write a poem about John Doe, them translate it into Italian.", shouldPrintPrompt, "SummarizePlugin", "WriterPlugin");
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public async Task RunBookSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Loops and Conditionals");

        await RunSampleAsync("Create a book with 3 chapters about a group of kids in a club called 'The Thinking Caps.'", shouldPrintPrompt, "WriterPlugin", "MiscPlugin");
    }

    public Example65_HandlebarsPlanner(ITestOutputHelper output) : base(output)
    {
    }
}
