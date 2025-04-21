// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.SemanticKernel.Planning.Handlebars;
using Plugins.DictionaryPlugin;
using Resources;

namespace Planners;

[TestClass]
public class HandlebarsPlanning : BaseTest
{
    private const string CourseraPluginName = "CourseraPlugin";

    private void WriteSampleHeading(string name)
    {
        Console.WriteLine($"======== [Handlebars Planner] Sample - Create and Execute Plan with: {name} ========");
    }

    private async Task<Kernel?> SetupKernelAsync(params string[] pluginDirectoryNames)
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                serviceId: "AzureOpenAIChat",
                apiKey: TestConfiguration.AzureOpenAI.ApiKey,
                modelId: TestConfiguration.AzureOpenAI.DeploymentName)
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
                await kernel.ImportPluginFromOpenApiAsync(CourseraPluginName, new Uri("https://www.coursera.org/api/rest/v1/search/openapi.yaml"));
            }
            else
            {
                string folder = RepoFiles.SamplePluginsPath();

                foreach (string pluginDirectoryName in pluginDirectoryNames)
                {
                    kernel.ImportPluginFromPromptDirectory(Path.Combine(folder, pluginDirectoryName));
                }
            }
        }

        return kernel;
    }

    private void PrintPlannerDetails(string goal, HandlebarsPlan plan, string result, bool shouldPrintPrompt)
    {
        Console.WriteLine($"Goal: {goal}");
        Console.WriteLine($"\nOriginal plan:\n{plan}");
        Console.WriteLine($"\nResult:\n{result}\n");

        // Print the prompt template
        if (shouldPrintPrompt && plan.Prompt is not null)
        {
            Console.WriteLine("\n======== CreatePlan Prompt ========");
            Console.WriteLine(plan.Prompt);
        }
    }

    private async Task RunSampleAsync(
        string goal,
        HandlebarsPlannerOptions? plannerOptions = null,
        KernelArguments? initialContext = null,
        bool shouldPrintPrompt = false,
        bool shouldInvokePlan = true,
        params string[] pluginDirectoryNames)
    {
        Kernel? kernel = await this.SetupKernelAsync(pluginDirectoryNames);

        if (kernel is null)
        {
            return;
        }

        plannerOptions ??= new HandlebarsPlannerOptions()
        {
            ExecutionSettings = new AzureOpenAIPromptExecutionSettings
            {
                Temperature = 0.0,
                TopP = 0.1
            }
        };

        plannerOptions.AllowLoops = true;

        HandlebarsPlanner planner = new(plannerOptions);

        HandlebarsPlan plan = await planner.CreatePlanAsync(kernel, goal, initialContext);

        string? result = shouldInvokePlan ? await plan.InvokeAsync(kernel, initialContext) : string.Empty;

        PrintPlannerDetails(goal, plan, result, shouldPrintPrompt);
    }

    [TestMethod]
    public async Task PlanNotPossibleSampleAsync()
    {
        WriteSampleHeading("Plan Not Possible");

        await RunSampleAsync("Send Mary an email with the list of meeting I have scheduled today.", null, null, true, false, "SummarizePlugin");
    }

    [TestMethod]
    public async Task RunCourseraSampleAsync()
    {
        WriteSampleHeading("Coursera OpenAPI Plugin");

        await RunSampleAsync("Show me courses about Artificial Intelligence.", null, null, true, false, CourseraPluginName);
    }

    [TestMethod]
    public async Task RunDictionaryWithBasicTypesSampleAsync()
    {
        WriteSampleHeading("Basic Types using Local Dictionary Plugin");

        await RunSampleAsync("Get a random word and its definition.", null, null, true, true, StringParamsDictionaryPlugin.PluginName);
    }

    [TestMethod]
    public async Task RunLocalDictionaryWithComplexTypesSampleAsync()
    {
        WriteSampleHeading("Complex Types using Local Dictionary Plugin");
        await RunSampleAsync("Teach me two random words and their definition.", null, null, true, true, ComplexParamsDictionaryPlugin.PluginName);
    }

    [TestMethod]

    public async Task RunPoetrySampleAsync()
    {
        WriteSampleHeading("Multiple Plugins");

        await RunSampleAsync("Write a poem about John Doe, then translate it into Italian.", null, null, false, true, "SummarizePlugin", "WriterPlugin");
    }

    [TestMethod]
    public async Task RunBookSampleAsync()
    {
        WriteSampleHeading("Loops and Conditionals");
        await RunSampleAsync("Create a book with 3 chapters about a group of kids in a club called 'The Thinking Caps.'", null, null, false, true, "WriterPlugin", "MiscPlugin");
    }

    [TestMethod]
    public async Task RunPredefinedVariablesSampleAsync()
    {
        WriteSampleHeading("CreatePlan Prompt With Predefined Variables");

        // When using predefined variables, you must pass these arguments to both the CreatePlanAsync and InvokeAsync methods.
        var initialArguments = new KernelArguments()
        {
            { "greetings", new List<string>(){ "hey", "bye" } },
            { "someNumber", 1 },
            { "person", new Dictionary<string, string>()
            {
                {"name", "John Doe" },
                { "language", "Italian" },
            } }
        };

        await RunSampleAsync("Write a poem about the given person, then translate it into French.", null, initialArguments, false, true, "WriterPlugin", "MiscPlugin");
    }

    [TestMethod]

    public async Task RunPromptWithAdditionalContextSampleAsync()
    {
        WriteSampleHeading("Prompt With Additional Context");

        // Pulling the raw content from SK's README file as domain context.
        static async Task<string> getDomainContext()
        {
            // For demonstration purposes only, beware of token count.
            var repositoryUrl = "https://github.com/microsoft/semantic-kernel";
            var readmeUrl = $"{repositoryUrl}/main/README.md".Replace("github.com", "raw.githubusercontent.com", StringComparison.CurrentCultureIgnoreCase);
            try
            {
                var httpClient = new HttpClient();
                // Send a GET request to the specified URL  
                var response = await httpClient.GetAsync(new Uri(readmeUrl));
                response.EnsureSuccessStatusCode(); // Throw an exception if not successful  

                // Read the response content as a string  
                var content = await response.Content.ReadAsStringAsync();
                httpClient.Dispose();
                return "Content imported from the README of https://github.com/microsoft/semantic-kernel:\n" + content;
            }
            catch (HttpRequestException e)
            {
                System.Console.WriteLine("\nException Caught!");
                System.Console.WriteLine("Message :{0} ", e.Message);
                return "";
            }
        }

        var goal = "Help me onboard to the Semantic Kernel SDK by creating a quick guide that includes a brief overview of the SDK for C# developers and detailed set-up steps. Include relevant links where possible. Then, draft an email with this guide, so I can share it with my team.";
        var plannerOptions = new HandlebarsPlannerOptions()
        {
            // Context to be used in the prompt template.
            GetAdditionalPromptContext = getDomainContext,
        };

        await RunSampleAsync(goal, plannerOptions, null, false, true, "WriterPlugin");
    }

    [TestMethod]
    public async Task RunOverrideCreatePlanPromptSampleAsync()
    {
        WriteSampleHeading("CreatePlan Prompt Override");

        static string OverridePlanPrompt()
        {
            // Load a custom CreatePlan prompt template from an embedded resource.
            var ResourceFileName = "65-prompt-override.handlebars";
            var fileContent = EmbeddedResource.ReadStream(ResourceFileName);
            return new StreamReader(fileContent!).ReadToEnd();
        }

        var plannerOptions = new HandlebarsPlannerOptions()
        {
            // Callback to override the default prompt template.
            CreatePlanPromptHandler = OverridePlanPrompt,
        };

        var goal = "I just watched the movie 'Inception' and I loved it! I want to leave a 5 star review. Can you help me?";

        // Note that since the custom prompt inputs a unique Helpers section with helpers not actually registered with the kernel,
        // any plan created using this prompt will fail execution; thus, we will skip the InvokePlan call in this example.
        // For a simpler example, see `ItOverridesPromptAsync` in the dotnet\src\Planners\Planners.Handlebars.UnitTests\Handlebars\HandlebarsPlannerTests.cs file.
        await RunSampleAsync(goal, plannerOptions, null, false, shouldInvokePlan: false, "WriterPlugin");
    }
}
