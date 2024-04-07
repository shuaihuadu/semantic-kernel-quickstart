namespace KernelSyntaxExamples;

public class Example65_HandlebarsPlanner : BaseTest
{
    private static int sampleIndex;

    private const string CourseraPluginName = "CourseraPlugin";

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public async Task PlanNotPossibleSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("Plan Not Possible");

        try
        {
            await RunSampleAsync("Send Mary an email with the list of meetings I have scheduled today.", null, null, shouldPrintPrompt, true, "SummarizePlugin");
        }
        catch (KernelException ex) when (
            ex.Message.Contains(nameof(HandlebarsPlannerErrorCodes.InsufficientFunctionsForGoal), StringComparison.CurrentCultureIgnoreCase)
            || ex.Message.Contains(nameof(HandlebarsPlannerErrorCodes.HallucinatedHelpers), StringComparison.CurrentCultureIgnoreCase)
            || ex.Message.Contains(nameof(HandlebarsPlannerErrorCodes.InvalidTemplate), StringComparison.CurrentCultureIgnoreCase))
        {
            WriteLine($"\n{ex.Message}\n");
        }
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(true)]
    public Task RunCourseraSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("Coursera OpenAPI Plugin");

        return RunSampleAsync("Show me coursera about Artificial Intelligence.", null, null, shouldPrintPrompt, true, CourseraPluginName);
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public Task RunDictioinaryWithBasicTypesSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("Basic Types using local Dictionary Plugin");

        return RunSampleAsync("Get a random word and its definitioin.", null, null, shouldPrintPrompt, true, StringParamsDictionaryPlugin.PluginName);
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(true)]
    public Task RunLocalDictionaryWithComplexTypesSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("Complex Types using local Dictionary Plugin");

        return RunSampleAsync("Teach me two random words and their definition.", null, null, shouldPrintPrompt, true, ComplexParamsDictionaryPlugin.PluginName);
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public Task RunPoetrySampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("Multiple Plugins");

        return RunSampleAsync("Write a poem about John Doe, then translate it into Italian.", null, null, shouldPrintPrompt, true, "SummarizePlugin", "WriterPlugin");
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(false)]
    public Task RunBookSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("Loops and Conditionals");

        return RunSampleAsync("Create a book with 3 chapters about a group of kids in a club called 'The Thinking Caps.'", null, null, shouldPrintPrompt, true, "WriterPlugin", "MiscPlugin");
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(true)]
    public Task RunPredefinedVariablesSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("CreatePlan Prompt With Predefined Variables");

        KernelArguments initialArguments = new KernelArguments()
        {
            {"",new List<string>(){ "hey","bye"} },
            {"sortNumber",1 },
            { "person",new Dictionary<string,string>()
                {
                    {"name","John Doe" },
                    { "language","Italian"}
                }
            }
        };

        return RunSampleAsync("Write a poem about the given person, then translate it into French.", null, initialArguments, shouldPrintPrompt, true, "WriterPlugin", "MiscPlugin");
    }

    [RetryTheory(typeof(HttpOperationException))]
    [InlineData(true)]
    public Task RunPromptWithAdditionalContextSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("Prompt With Additional Context");

        static async Task<string> getDomainContext()
        {
            string repositoryUrl = "https://github.com/microsoft/semantic-kernel";

            string readmeUrl = $"{repositoryUrl}/main/README.md".Replace("github.com", "raw.githubusercontent.com", StringComparison.CurrentCultureIgnoreCase);

            try
            {
                HttpClient httpClient = new();

                HttpResponseMessage response = await httpClient.GetAsync(repositoryUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                httpClient.Dispose();

                return "Content imported from the README of https://github.com/microsoft/semantic-kernel:\n" + content;
            }
            catch (HttpRequestException ex)
            {

                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message: {0}", ex.Message);
                return string.Empty;
            }
        }

        string goal = "Help me onboard to the Semantic Kernel SDK by creating a quick guide that includes a brief overview of the SDK for C# developers and detailed set-up steps. Include relevant links where possible. Then, draft an email with this guide, so I can share it with my team.";

        HandlebarsPlannerOptions plannerOptions = new()
        {
            GetAdditionalPromptContext = getDomainContext
        };

        return RunSampleAsync(goal, plannerOptions, null, shouldPrintPrompt, true, "WriterPlugin");
    }

    [RetryTheory(typeof(HttpOperationException), Skip = "TODO CreatePlanPromptHandler")]
    [InlineData(true)]
    public Task RunOverrideCreatePlanPromptSampleAsync(bool shouldPrintPrompt)
    {
        WriteSampleHeading("CreatePkan Prompt Override");

        static string OverridePlanPrompt()
        {
            string resourceFileName = "65-prompt-override.handlebars";

            Stream? fileContent = EmbeddedResource.ReadStream(resourceFileName);

            return new StreamReader(fileContent!).ReadToEnd();
        }

        HandlebarsPlannerOptions plannerOptions = new HandlebarsPlannerOptions
        {
            //TODO CreatePlanPromptHandler
        };

        string goal = "I just watched the movie 'Inception' and I loved it! I want to leave a 5 start review. Can you help me?";

        return RunSampleAsync(goal, plannerOptions, null, shouldPrintPrompt, false, "WriterPlugin");
    }

    private void WriteSampleHeading(string name)
    {
        WriteLine($"======== [Handlebars Planner] Sample {sampleIndex++} - Coursera and Execute Plan with: {name} ========");
    }

    private async Task<Kernel?> SetupKernelAsync(params string[] pluginDirectoryNames)
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                serviceId: "AzureOpenAIChat",
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
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
                await kernel.ImportPluginFromOpenApiAsync(
                    CourseraPluginName,
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

        return kernel;
    }

    private void PrintPlannerDetails(string goal, HandlebarsPlan plan, string result, bool shouldPrintPrompt)
    {
        WriteLine($"Goal: {goal}");
        WriteLine($"\nOriginal plan:\n{plan}");
        WriteLine($"\nResult:\n{result}\n");

        if (shouldPrintPrompt && plan.Prompt is not null)
        {
            WriteLine("\n======== CreatePlan Prompt ========");
            WriteLine(plan.Prompt);
        }
    }

    private async Task RunSampleAsync(string goal,
        HandlebarsPlannerOptions? plannerOptions = null,
        KernelArguments? initialContext = null,
        bool shouldPrintPrompt = false,
        bool shouldInvokePlan = true,
        params string[] pluginDirectoryNames)
    {
        Kernel? kernel = await SetupKernelAsync(pluginDirectoryNames);

        if (kernel is null)
        {
            return;
        }

        plannerOptions ??= new HandlebarsPlannerOptions()
        {
            ExecutionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.0,
                TopP = 0.1,
            }
        };

        plannerOptions.AllowLoops = TestConfiguration.AzureOpenAI.DeploymentName.Contains("gpt-4", StringComparison.OrdinalIgnoreCase);

        HandlebarsPlanner planner = new HandlebarsPlanner(plannerOptions);

        HandlebarsPlan plan = await planner.CreatePlanAsync(kernel, goal, initialContext);

        string result = shouldInvokePlan ? await plan.InvokeAsync(kernel, initialContext) : string.Empty;

        PrintPlannerDetails(goal, plan, result, shouldPrintPrompt);
    }

    public Example65_HandlebarsPlanner(ITestOutputHelper output) : base(output)
    {
    }
}
