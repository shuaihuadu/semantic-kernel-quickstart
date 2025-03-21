using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Agents;

namespace Agents;

[TestClass]
public class ChatCompletion_FunctionTermination : BaseAgentsTest
{
    [TestMethod]
    public async Task UseAutoFunctionInvocationFilterWithAgentInvocationAsync()
    {
        ChatCompletionAgent agent = new()
        {
            Instructions = "Answer questions about the menu.",
            Kernel = CreateKernelWithFilter(),
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
        };

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        ChatHistory chat = [];

        await InvokeAgentAsync("Hello");
        await InvokeAgentAsync("What is the special soup?");
        await InvokeAgentAsync("What is the special drink?");
        await InvokeAgentAsync("Thank you");

        WriteChatHistory(chat);

        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.Add(message);
            this.WriteAgentChatMessage(message);

            await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
            {
                if (!response.Items.Any(i => i is FunctionCallContent || i is FunctionResultContent))
                {
                    chat.Add(response);
                }

                this.WriteAgentChatMessage(response);
            }
        }
    }

    [TestMethod]
    public async Task UseAutoFunctionInvocationFilterWithAgentChatAsync()
    {
        ChatCompletionAgent agent = new()
        {
            Instructions = "Answer questions about the menu.",
            Kernel = CreateKernelWithFilter(),
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
        };

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        AgentGroupChat chat = new();

        await InvokeAgentAsync("Hello");
        await InvokeAgentAsync("What is the special soup?");
        await InvokeAgentAsync("What is the special drink?");
        await InvokeAgentAsync("Thank you");

        WriteChatHistory(await chat.GetChatMessagesAsync().ToArrayAsync());

        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.AddChatMessage(message);
            this.WriteAgentChatMessage(message);

            await foreach (ChatMessageContent response in chat.InvokeAsync(agent))
            {
                this.WriteAgentChatMessage(response);
            }
        }
    }

    [TestMethod]
    public async Task UseAutoFunctionInvocationFilterWithStreamingAgentInvocationAsync()
    {
        ChatCompletionAgent agent = new()
        {
            Instructions = "Answer questions about the menu.",
            Kernel = CreateKernelWithFilter(),
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
        };

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        ChatHistory chat = [];

        await InvokeAgentAsync("Hello");
        await InvokeAgentAsync("What is the special soup?");
        await InvokeAgentAsync("What is the special drink?");
        await InvokeAgentAsync("Thank you");

        WriteChatHistory(chat);

        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.Add(message);
            this.WriteAgentChatMessage(message);

            int historyCount = chat.Count;

            bool isFirst = false;

            await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
            {
                if (string.IsNullOrEmpty(response.Content))
                {
                    continue;
                }

                if (!isFirst)
                {
                    Console.WriteLine($"\n# {response.Role} - {response.AuthorName ?? "*"}");
                    isFirst = true;
                }

                Console.Write($"\t > streamed: '{response.Content}'");
            }

            if (historyCount <= chat.Count)
            {
                for (int index = 0; index < chat.Count; index++)
                {
                    this.WriteAgentChatMessage(chat[index]);
                }
            }
        }
    }

    [TestMethod]
    public async Task UseAutoFunctionInvocationFilterWithStreamingAgentChatAsync()
    {
        ChatCompletionAgent agent =
            new()
            {
                Instructions = "Answer questions about the menu.",
                Kernel = CreateKernelWithFilter(),
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            };

        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        AgentGroupChat chat = new();

        await InvokeAgentAsync("Hello");
        await InvokeAgentAsync("What is the special soup?");
        await InvokeAgentAsync("What is the special drink?");
        await InvokeAgentAsync("Thank you");

        WriteChatHistory(await chat.GetChatMessagesAsync().ToArrayAsync());

        async Task InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            chat.AddChatMessage(message);
            this.WriteAgentChatMessage(message);

            bool isFirst = false;
            await foreach (StreamingChatMessageContent response in chat.InvokeStreamingAsync(agent))
            {
                if (string.IsNullOrEmpty(response.Content))
                {
                    continue;
                }

                if (!isFirst)
                {
                    Console.WriteLine($"\n# {response.Role} - {response.AuthorName ?? "*"}:");
                    isFirst = true;
                }

                Console.WriteLine($"\t > streamed: '{response.Content}'");
            }
        }
    }

    private void WriteChatHistory(IEnumerable<ChatMessageContent> chat)
    {
        Console.WriteLine("================================");
        Console.WriteLine("CHAT HISTORY");
        Console.WriteLine("================================");
        foreach (ChatMessageContent message in chat)
        {
            this.WriteAgentChatMessage(message);
        }
    }

    private Kernel CreateKernelWithFilter()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();

        base.AddChatCompletionToKernel(builder);

        builder.Services.AddSingleton<IAutoFunctionInvocationFilter>(new AutoInvocationFilter());

        return builder.Build();
    }

    private sealed class MenuPlugin
    {
        [KernelFunction, Description("Provides a list of specials from the menu.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Too smart")]
        public string GetSpecials()
        {
            return
                """
                Special Soup: Clam Chowder
                Special Salad: Cobb Salad
                Special Drink: Chai Tea
                """;
        }

        [KernelFunction, Description("Provides the price of the requested menu item.")]
        public string GetItemPrice([Description("The name of the menu item.")] string menuItem)
        {
            return "$9.99";
        }
    }

    private class AutoInvocationFilter(bool terminate = true) : IAutoFunctionInvocationFilter
    {
        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            await next(context);

            if (context.Function.PluginName == nameof(MenuPlugin))
            {
                context.Terminate = terminate;
            }
        }
    }
}
