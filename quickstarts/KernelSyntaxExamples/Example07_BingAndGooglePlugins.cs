﻿namespace KernelSyntaxExamples;

public static class Example07_BingAndGooglePlugins
{
    public static async Task RunAsync()
    {
        string deploymentName = TestConfiguration.AzureOpenAI.ChatDeploymentName;
        string endpoint = TestConfiguration.AzureOpenAI.Endpoint;
        string apiKey = TestConfiguration.AzureOpenAI.ApiKey;

        if (string.IsNullOrEmpty(deploymentName) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Azure OpenAI credentials not found. Skipping example.");

            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
            .Build();

        string bingApiKey = TestConfiguration.Bing.ApiKey;

        if (string.IsNullOrEmpty(bingApiKey))
        {
            Console.WriteLine("Bing credentials not found. Skipping example.");
        }
        else
        {
#pragma warning disable SKEXP0054 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            BingConnector bingConnector = new(bingApiKey);
            WebSearchEnginePlugin webSearchEngine = new(bingConnector);
#pragma warning restore SKEXP0054 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            kernel.ImportPluginFromObject(webSearchEngine, "bing");

            await Example1Async(kernel, "bing");
            await Example2Async(kernel);
        }
    }

    private static async Task Example1Async(Kernel kernel, string searchPluginName)
    {
        Console.WriteLine("======== Bing and Google Search Plugins ========");

        //string question = "What's the largest building in the world?";
        string question = "What's the capital of China?";

        KernelFunction function = kernel.Plugins[searchPluginName]["search"];

        FunctionResult result = await kernel.InvokeAsync(function, new() { ["query"] = question });

        Console.WriteLine(question);
        Console.WriteLine($"----- {searchPluginName} -----");
        Console.WriteLine(result.GetValue<string>());
    }

    private static async Task Example2Async(Kernel kernel)
    {
        Console.WriteLine("======== Use Search Plugin to answer user questions ========");

        const string SemanticFunction = @"Answer questions only when you know the facts or the information is provided.
When you don't have sufficient information you reply with a list of commands to find the information needed.
When answering multiple questions, use a bullet point list.
Note: make sure single and double quotes are escaped using a backslash char.

[COMMANDS AVAILABLE]
- bing.search

[INFORMATION PROVIDED]
{{ $externalInformation }}

[EXAMPLE 1]
Question: what's the biggest lake in Italy?
Answer: Lake Garda, also known as Lago di Garda.

[EXAMPLE 2]
Question: what's the biggest lake in Italy? What's the smallest positive number?
Answer:
* Lake Garda, also known as Lago di Garda.
* The smallest positive number is 1.

[EXAMPLE 3]
Question: what's Ferrari stock price? Who is the current number one female tennis player in the world?
Answer:
{{ '{{' }} bing.search ""what\\'s Ferrari stock price?"" {{ '}}' }}.
{{ '{{' }} bing.search ""Who is the current number one female tennis player in the world?"" {{ '}}' }}.

[END OF EXAMPLES]

[TASK]
Question: {{ $question }}.
Answer: ";

        string question = "Who is the most followed person on Douyin right now? What's the exchange rate EUR:USD?";

        KernelFunction oracle = kernel.CreateFunctionFromPrompt(SemanticFunction, new PromptExecutionSettings
        {
            ModelId = string.Empty,
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 200,
                ["temperature"] = 0.0
            }
        });

        FunctionResult answer = await kernel.InvokeAsync(oracle, new()
        {
            ["question"] = question,
            ["externalInformation"] = string.Empty
        });

        string result = answer.GetValue<string>()!;

        if (result.Contains("bing.search", StringComparison.OrdinalIgnoreCase))
        {
            KernelPromptTemplateFactory promptTemplateFactory = new KernelPromptTemplateFactory();

            IPromptTemplate promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(result));

            Console.WriteLine("---- Fetching information from Bing...");

            string information = await promptTemplate.RenderAsync(kernel);

            Console.WriteLine("Information found:");
            Console.WriteLine(information);

            answer = await kernel.InvokeAsync(oracle, new()
            {
                ["question"] = question,
                ["externalInformation"] = information
            });
        }
        else
        {
            Console.WriteLine("AI had all the information, no need to query Bing.");
        }

        Console.WriteLine("----- ANSWER:");
        Console.Write(answer.GetValue<string>());
    }
}
