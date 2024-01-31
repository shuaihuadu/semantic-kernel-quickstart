namespace KernelSyntaxExamples;

public class Example74_FlowOrchestrator : BaseTest
{
    [Fact]
    public Task RunAsync()
    {
        this.WriteLine($"Loading {typeof(SimpleCalculatorPlugin).AssemblyQualifiedName}");

        return RunExampleAsync();
    }

    private async Task RunExampleAsync()
    {
        BingConnector bingConnector = new(TestConfiguration.Bing.ApiKey);

        WebSearchEnginePlugin webSearchEnginePlugin = new(bingConnector);

        using ILoggerFactory loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder.AddConsole().AddFilter(null, LogLevel.Error));

        Dictionary<object, string?> plugins = new()
        {
            {webSearchEnginePlugin,"WebSearch" },
            {new TimePlugin(),"Time" }
        };

        FlowOrchestrator orchestrator = new(
            GetKernelBuilder(loggerFactory),
            await FlowStatusProvider.ConnectAsync(new VolatileMemoryStore()).ConfigureAwait(false),
            plugins,
            config: GetOrchestratorConfig());

        string sessionId = Guid.NewGuid().ToString();

        this.WriteLine("*****************************************************");
        this.WriteLine($"Executing {nameof(RunExampleAsync)}");

        Stopwatch sw = new();
        sw.Start();

        this.WriteLine("Flow: " + flow.Name);

        string question = flow.Steps.First().Goal;

        FunctionResult result = await orchestrator.ExecuteFlowAsync(flow, sessionId, question).ConfigureAwait(false);

        this.WriteLine("Question: " + question);
        this.WriteLine("Answer: " + result.Metadata!["answer"]);
        this.WriteLine("Assistant: " + result.GetValue<List<string>>()!.Single());

        string[] userInputs =  {
            "my email is bad*email&address",
            "my email is sample@xyz.com",
            "yes",
            "I also want to notify foo@bar.com",
            "no I don't need notify any more address"
        };

        foreach (string input in userInputs)
        {
            this.WriteLine($"User: {input}");

            result = await orchestrator.ExecuteFlowAsync(flow, sessionId, input).ConfigureAwait(false);

            List<string> responses = result.GetValue<List<string>>()!;

            foreach (string response in responses)
            {
                this.WriteLine("Assistant: " + response);
            }

            if (result.IsComplete(flow))
            {
                break;
            }
        }

        this.WriteLine("\t Email Address: " + result.Metadata!["email_addresses"]);
        this.WriteLine("\t Email Payload: " + result.Metadata!["email"]);

        this.WriteLine("Time Taken: " + sw.Elapsed);
        this.WriteLine("*****************************************************");
    }

    private static IKernelBuilder GetKernelBuilder(ILoggerFactory loggerFactory)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        return builder.AddAzureOpenAIChatCompletion(
            TestConfiguration.AzureOpenAI.DeploymentName,
            TestConfiguration.AzureOpenAI.Endpoint,
            TestConfiguration.AzureOpenAI.ApiKey);
    }

    private static FlowOrchestratorConfig GetOrchestratorConfig()
    {
        FlowOrchestratorConfig config = new()
        {
            MaxStepIterations = 20
        };

        return config;
    }

    public sealed class ChatPlugin
    {
        private const string Goal = "Prompt user to provide a valid email address";

        private const string EmailRegex = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";

        private const string SystemPrompt =
            $@"I am AI assistant and will only answer questions related to collect email.
The email should conform the regex: {EmailRegex}

If I cannot answer, say that I don't know.

# IMPORTANT
Do not expose the regex in your response.
";

        private readonly IChatCompletionService chatCompletionService;

        private int MaxTokens { get; set; } = 256;

        private readonly PromptExecutionSettings promptExecutionSettings;

        public ChatPlugin(Kernel kernel)
        {
            this.chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            this.promptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = this.MaxTokens,
                StopSequences = new List<string>() { "Observation:" },
                Temperature = 0
            };
        }

        [KernelFunction("ConfigureEmailAddress")]
        [Description("Useful to assist in configuration of email address, must be called after email provided")]
        public async Task<string> CollectEmailAsync(
            [Description("The email address provided by the user, pass no matter what the value is")] string email_addresses,
            KernelArguments arguments)
        {
            ChatHistory chat = new();
            chat.AddUserMessage(Goal);

            ChatHistory? chatHistory = arguments.GetChatHistory();

            if (chatHistory?.Count > 0)
            {
                chat.AddRange(chatHistory);
            }

            if (!string.IsNullOrEmpty(email_addresses) && IsValidEmail(email_addresses))
            {
                return "Thanks for providing the info, the following email would be used in subsequent steps: " + email_addresses;
            }

            arguments["email_addresses"] = string.Empty;
            arguments.PromptInput();

            ChatMessageContent response = await this.chatCompletionService.GetChatMessageContentAsync(chat).ConfigureAwait(false);
            return response.Content ?? string.Empty;
        }

        private bool IsValidEmail(string email)
        {
            Regex regex = new(EmailRegex);
            return regex.IsMatch(email);
        }
    }

    public sealed class EmailPluginV2
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        [KernelFunction]
        [Description("Send email")]
        public string SendEmail(
            [Description("target email addresses")] string emailAddresses,
            [Description("answer, which is going to be the email content")] string answer,
            KernelArguments arguments)
        {
            Email contract = new()
            {
                Address = emailAddresses,
                Content = answer
            };

            string emailPayload = JsonSerializer.Serialize(contract, options);
            arguments["email"] = emailPayload;

            return "Here's the API contract I will post to mail server: " + emailPayload;
        }

        private sealed class Email
        {
            public string? Address { get; set; }

            public string? Content { get; set; }
        }
    }

    private static readonly Flow flow = FlowSerializer.DeserializeFromYaml(@"
name: FlowOrchestrator_Example_Flow
goal: answer question and send email
steps:
  - goal: What is the tallest mountain in Asia? How tall is it divided by 2?
    plugins:
      - WebSearchEnginePlugin
      - LanguageCalculatorPlugin
    provides:
      - answer
  - goal: Collect email address
    plugins:
      - ChatPlugin
    completionType: AtLeastOnce
    transitionMessage: do you want to send it to another email address?
    provides:
      - email_addresses

  - goal: Send email
    plugins:
      - EmailPluginV2
    requires:
      - email_addresses
      - answer
    provides:
      - email

provides:
    - email
");

    public Example74_FlowOrchestrator(ITestOutputHelper output) : base(output)
    {
    }
}
