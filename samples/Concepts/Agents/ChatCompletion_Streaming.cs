using Microsoft.SemanticKernel.Agents;

namespace Agents;

[TestClass]
public class ChatCompletion_Streaming : BaseAgentsTest
{
    private const string ParrotName = "Parrot";
    private const string ParrotInstructions = "Repeat the user message in the voice of a pirate and then end with a parrot sound.";

    [TestMethod]
    public async Task UseStreamingChatCompletionAgentAsync()
    {
        ChatCompletionAgent agent = new()
        {
            Name = ParrotName,
            Instructions = ParrotInstructions,
            Kernel = this.CreateKernelWithChatCompletion()
        };

        ChatHistory chat = [];

        await InvokeAgentAsync(agent, chat, "Fortune favors the bold.");
        await InvokeAgentAsync(agent, chat, "I came, I saw, I conquered.");
        await InvokeAgentAsync(agent, chat, "Practice makes perfect.");

        DisplayChatHistory(chat);
    }

    [TestMethod]
    public async Task UseStreamingChatCompletionAgentWithPluginAsync()
    {
        const string MenuInstructions = "Answer questions about the menu.";

        ChatCompletionAgent agent = new()
        {
            Name = "Host",
            Instructions = MenuInstructions,
            Kernel = this.CreateKernelWithChatCompletion(),
            Arguments = new KernelArguments(new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
        };

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        ChatHistory chat = [];

        await InvokeAgentAsync(agent, chat, "What is the special soup?");
        await InvokeAgentAsync(agent, chat, "What is the special drink?");

        DisplayChatHistory(chat);
    }

    private async Task InvokeAgentAsync(ChatCompletionAgent agent, ChatHistory chat, string input)
    {
        ChatMessageContent message = new(AuthorRole.User, input);
        chat.Add(message);
        this.WriteAgentChatMessage(message);

        int historyCount = chat.Count;

        bool isFirst = false;

        await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(chat))
        {
            if (string.IsNullOrEmpty(response.Content))
            {
                StreamingFunctionCallUpdateContent? functionCall = response.Items.OfType<StreamingFunctionCallUpdateContent>().SingleOrDefault();

                if (!string.IsNullOrEmpty(functionCall?.Name))
                {
                    Console.WriteLine($"\n# {response.Role} - {response.AuthorName ?? "*"}: FUNCTION CALL - {functionCall.Name}");
                }

                continue;
            }

            if (!isFirst)
            {
                Console.WriteLine($"\n# {response.Role} - {response.AuthorName ?? "*"}:");
                isFirst = true;
            }

            Console.WriteLine($"\t > streamed: '{response.Content}'");
        }

        if (historyCount <= chat.Count)
        {
            for (int index = 0; index < chat.Count; index++)
            {
                this.WriteAgentChatMessage(chat[index]);
            }
        }
    }

    private void DisplayChatHistory(ChatHistory history)
    {
        Console.WriteLine("================================");
        Console.WriteLine("CHAT HISTORY");
        Console.WriteLine("================================");

        foreach (ChatMessageContent message in history)
        {
            this.WriteAgentChatMessage(message);
        }
    }

    public sealed class MenuPlugin
    {
        [KernelFunction, Description("Provides a list of specials from the menu.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Too smart")]
        public string GetSpecials()
        {
            return @"
Special Soup: Clam Chowder
Special Salad: Cobb Salad
Special Drink: Chai Tea
";
        }

        [KernelFunction, Description("Provides the price of the requested menu item.")]
        public string GetItemPrice(
            [Description("The name of the menu item.")]
        string menuItem)
        {
            return "$9.99";
        }
    }
}
