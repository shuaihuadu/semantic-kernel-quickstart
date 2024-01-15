namespace KernelSyntaxExamples;

public static class Example65_HandlebarsPlanner
{
    private static int sampleIndex;
    private const string CourseraPluginName = "CourseraPlugin";

    public static async Task RunAsync()
    {
        sampleIndex = 1;

        await RunDictionaryWithBasicTypesSampleAsync();
        await RunPoetrySampleAsync();
        await RunBookSampleAsync();

        await PlanNotPossibleSampleAsync();

        await RunLocalDictionaryWithComplexTypesSampleAsync();

        await RunCourseraSampleAsync();
    }

    private static void WriteSampleHeadingToConsole(string name)
    {
        Console.WriteLine($"======== [Handlebars Planner] Sample {sampleIndex++} -  Create and Execute Plan with: {name} ========");
    }

    private static async Task RunSampleAsync(string goal, bool shouldPrintPrompt = false, params string[] pluginDirectoryNames)
    {
        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;
        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;

        if (deploymentName == null
            || endpoint == null
            || apiKey == null)
        {
            Console.WriteLine("AzureOpenAI endpoint, apiKey, or deploymentName not found. Skipping example.");
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

        Console.WriteLine($"Goal: {goal}");

        HandlebarsPlan plan = await planner.CreatePlanAsync(kernel, goal);

        if (shouldPrintPrompt && plan.Prompt is not null)
        {
            Console.WriteLine($"\nPrompt template:\n{plan.Prompt}");
        }

        string result = await plan.InvokeAsync(kernel);
        Console.WriteLine($"\nResult:\n{result}\n");
    }

    private static async Task PlanNotPossibleSampleAsync(bool shouldPrintPrompt = false)
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

            Console.WriteLine($"\n${ex.Message}\n");
        }
    }

    private static async Task RunCourseraSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Coursera OpenAI Plugin");

        await RunSampleAsync("Show me courses about Artificial Intelligence.", shouldPrintPrompt, CourseraPluginName);
    }

    private static async Task RunDictionaryWithBasicTypesSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Basic Type using Local Dictionary Plugin");

        await RunSampleAsync("Get a random word and its definition.", shouldPrintPrompt, StringParamsDictionaryPlugin.PluginName);
    }

    private static async Task RunLocalDictionaryWithComplexTypesSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Complex Type using Local Dictionary Plugin");

        await RunSampleAsync("Teach me two random words and their definition.", shouldPrintPrompt, ComplexParamsDictionaryPlugin.PluginName);
    }

    private static async Task RunPoetrySampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Multiple Plugins");

        await RunSampleAsync("Write a poem about John Doe, them translate it into Italian.", shouldPrintPrompt, "SummarizePlugin", "WriterPlugin");
    }

    private static async Task RunBookSampleAsync(bool shouldPrintPrompt = false)
    {
        WriteSampleHeadingToConsole("Loops and Conditionals");

        await RunSampleAsync("Create a book with 3 chapters about a group of kids in a club called 'The Thinking Caps.'", shouldPrintPrompt, "WriterPlugin", "MiscPlugin");
    }
}
