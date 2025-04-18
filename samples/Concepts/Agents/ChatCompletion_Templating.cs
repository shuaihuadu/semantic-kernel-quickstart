using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.PromptTemplates.Liquid;

namespace Agents;

[TestClass]
public class ChatCompletion_Templating : BaseAgentsTest
{
    private readonly static (string Input, string? Style)[] s_inputs =
    [
        (Input: "Home cooking is great.", Style: null),
        (Input: "Talk about world peace.", Style: "iambic pentameter"),
        (Input: "Say something about doing your best.", Style: "e. e. cummings"),
        (Input: "What do you think about having fun?", Style: "old school rap")
    ];

    [TestMethod]
    public async Task InvokeAgentWithInstructionsTemplateAsync()
    {
        // Instruction based template always processed by KernelPromptTemplateFactory
        ChatCompletionAgent agent =
            new()
            {
                Kernel = this.CreateKernelWithChatCompletion(),
                Instructions =
                    """
                    Write a one verse poem on the requested topic in the style of {{$style}} use Chinese.
                    Always state the requested style of the poem.
                    """,
                Arguments = new KernelArguments()
                {
                    {"style", "haiku"}
                }
            };

        await InvokeChatCompletionAgentWithTemplateAsync(agent);
    }

    [TestMethod]
    public async Task InvokeAgentWithKernelTemplateAsync()
    {
        // Default factory is KernelPromptTemplateFactory
        await InvokeChatCompletionAgentWithTemplateAsync(
            """
            Write a one verse poem on the requested topic in the style of {{$style}} use Chinese.
            Always state the requested style of the poem.
            """,
            PromptTemplateConfig.SemanticKernelTemplateFormat,
            new KernelPromptTemplateFactory());
    }

    [TestMethod]
    public async Task InvokeAgentWithHandlebarsTemplateAsync()
    {
        await InvokeChatCompletionAgentWithTemplateAsync(
            """
            Write a one verse poem on the requested topic in the style of {{style}} use Chinese.
            Always state the requested style of the poem.
            """,
            HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            new HandlebarsPromptTemplateFactory());
    }

    [TestMethod]
    public async Task InvokeAgentWithLiquidTemplateAsync()
    {
        await InvokeChatCompletionAgentWithTemplateAsync(
            """
            Write a one verse poem on the requested topic in the style of {{style}} use Chinese.
            Always state the requested style of the poem.
            """,
            LiquidPromptTemplateFactory.LiquidTemplateFormat,
            new LiquidPromptTemplateFactory());
    }

    private async Task InvokeChatCompletionAgentWithTemplateAsync(
        string instructionTemplate,
        string templateFormat,
        IPromptTemplateFactory templateFactory)
    {
        // Define the agent
        PromptTemplateConfig templateConfig =
            new()
            {
                Template = instructionTemplate,
                TemplateFormat = templateFormat,
            };
        ChatCompletionAgent agent =
            new(templateConfig, templateFactory)
            {
                Kernel = this.CreateKernelWithChatCompletion(),
                Arguments = new KernelArguments()
                {
                    {"style", "haiku"}
                }
            };

        await InvokeChatCompletionAgentWithTemplateAsync(agent);
    }

    private async Task InvokeChatCompletionAgentWithTemplateAsync(ChatCompletionAgent agent)
    {
        ChatHistory chat = [];

        foreach ((string input, string? style) in s_inputs)
        {
            // Add input to chat
            ChatMessageContent request = new(AuthorRole.User, input);
            chat.Add(request);
            this.WriteAgentChatMessage(request);

            KernelArguments? arguments = null;

            if (!string.IsNullOrWhiteSpace(style))
            {
                // Override style template parameter
                arguments = new() { { "style", style } };
            }

            // Process agent response
            await foreach (ChatMessageContent message in agent.InvokeAsync(chat, arguments))
            {
                chat.Add(message);
                this.WriteAgentChatMessage(message);
            }
        }
    }
}
