namespace KernelSyntaxExamples;

public sealed class Example79_ChatCompletionAgent : BaseTest
{
    [Fact]
    public async Task ChatWithAgentAsync()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        ChatCompletionAgent agent = new(kernel,
            instructions: "You act as a professional financial adviser. However, clients may not know the terminology, so please provide a simple explanation.",
            new OpenAIPromptExecutionSettings
            {
                MaxTokens = 50,
                Temperature = 0.7,
                TopP = 1.0,
                PresencePenalty = 0.0,
                FrequencyPenalty = 0.0
            });

        string prompt = PrintPrompt("I need help with my investment portfolio. Please guide me.");

        PrintConversation(await agent.InvokeAsync(new[] { new ChatMessageContent(AuthorRole.User, prompt) }));
    }


    [Fact]
    public async Task TurnBasedAgentsChatAsync()
    {
        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 1500,
            Temperature = 0.7,
            TopP = 1.0,
            PresencePenalty = 0.0,
            FrequencyPenalty = 0.0
        };

        ChatCompletionAgent fitnessTrainer = new(
            kernel,
           instructions: "As a fitness trainer, suggest workout routines, and exercises for beginners. " +
           "You are not a stress management expert, so refrain from recommending stress management strategies. " +
           "Collaborate with the stress management expert to create a holistic wellness plan." +
           "Always incorporate stress reduction techniques provided by the stress management expert into the fitness plan." +
           "Always include your role at the beginning of each response, such as 'As a fitness trainer.",
           settings);

        ChatCompletionAgent stressManagementExpert = new(
            kernel,
            instructions: "As a stress management expert, provide guidance on stress reduction strategies. " +
            "Collaborate with the fitness trainer to create a simple and holistic wellness plan." +
            "You are not a fitness expert; therefore, avoid recommending fitness exercises." +
            "If the plan is not aligned with recommended stress reduction plan, ask the fitness trainer to rework it to incorporate recommended stress reduction techniques. " +
            "Only you can stop the conversation by saying WELLNESS_PLAN_COMPLETE if suggested fitness plan is good." +
            "Always include your role at the beginning of each response such as 'As a stress management expert.",
            settings);


        TurnBasedChat chat = new(new[] { fitnessTrainer, stressManagementExpert }, (chatHistory, replies, turn) =>
            turn >= 10 || replies.Any(message => message.Role == AuthorRole.Assistant && message.Content!.Contains("WELLNESS_PLAN_COMPLETE", StringComparison.InvariantCulture)));

        string prompt = "I need help creating a simple wellness plan for a beginner. Please guide me.";

        PrintConversation(await chat.SendMessageAsync(prompt));
    }

    private string PrintPrompt(string prompt)
    {
        WriteLine($"Prompt: {prompt}");

        return prompt;
    }

    private void PrintConversation(IEnumerable<ChatMessageContent> messages)
    {
        foreach (var message in messages)
        {
            WriteLine($"------------------------------- {message.Role} ------------------------------");
            WriteLine(message.Content);
            WriteLine();
        }
    }

    private sealed class TurnBasedChat
    {
        private readonly ChatCompletionAgent[] _agents;
        private readonly Func<ChatHistory, IEnumerable<ChatMessageContent>, int, bool> _exitCondition;

        public TurnBasedChat(IEnumerable<ChatCompletionAgent> agents, Func<ChatHistory, IEnumerable<ChatMessageContent>, int, bool> exitCondition)
        {
            this._agents = agents.ToArray();
            this._exitCondition = exitCondition;
        }

        public async Task<IReadOnlyList<ChatMessageContent>> SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            ChatHistory chat = new();
            chat.AddUserMessage(message);

            IReadOnlyList<ChatMessageContent> result = new List<ChatMessageContent>();

            int turn = 0;

            do
            {
                ChatCompletionAgent agent = this._agents[turn % this._agents.Length];

                result = await agent.InvokeAsync(chat, cancellationToken);

                chat.AddRange(result);

                turn++;
            }
            while (!this._exitCondition(chat, result, turn));

            return chat;
        }
    }

    public Example79_ChatCompletionAgent(ITestOutputHelper output) : base(output)
    {
    }
}
