namespace KernelSyntaxExamples;

public class Example59_OpenAIFunctionCalling(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        IKernelBuilder builder = KernelHelper.AzureOpenAIChatCompletionKernelBuilder();

        builder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Trace));

        Kernel kernel = builder.Build();

        kernel.ImportPluginFromFunctions("HelperFunctions", [
            kernel.CreateFunctionFromMethod(()=>DateTime.UtcNow.ToString("R"),"GetCurrentUtcTime","Retrieves the current time in UTC."),
            kernel.CreateFunctionFromMethod((string cityName)=>
                cityName switch
                {
                    "Boston" => "61 and rainy",
                    "London" => "55 and cloudy",
                    "Miami" => "80 and sunny",
                    "Paris" => "60 and rainy",
                    "Tokyo" => "50 and sunny",
                    "Sydney" => "75 and sunny",
                    "Tel Aviv" => "80 and sunny",
                    _=> "31 and snowing"
                },"Get_Weather_For_City","Gets the current weather for the specified city")
        ]);

        WriteLine("======== Example 1: Use automated function calling with a non-streaming prompt ========");
        {
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            WriteLine(await kernel.InvokePromptAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(settings)));

            WriteLine();
        }
        WriteLine("======== Example 2: Use automated function calling with a streaming prompt ========");
        {
            OpenAIPromptExecutionSettings setting = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            await foreach (var update in kernel.InvokePromptStreamingAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(setting)))
            {
                Write(update);
            }

            WriteLine();
        }
        WriteLine("======== Example 3: Use manual function calling with a non-streaming prompt ========");
        {
            IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = [];

            OpenAIPromptExecutionSettings setting = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            chatHistory.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

            while (true)
            {
                OpenAIChatMessageContent result = (OpenAIChatMessageContent)await chat.GetChatMessageContentAsync(chatHistory, setting, kernel);

                if (result.Content is not null)
                {
                    Write(result.Content);
                }

                List<ChatCompletionsFunctionToolCall> toolCalls = result.ToolCalls.OfType<ChatCompletionsFunctionToolCall>().ToList();

                if (toolCalls.Count == 0)
                {
                    break;
                }

                chatHistory.Add(result);

                foreach (var toolCall in toolCalls)
                {
                    string content = kernel.Plugins.TryGetFunctionAndArguments(toolCall, out KernelFunction? function, out KernelArguments? argument)
                        ? JsonSerializer.Serialize((await function.InvokeAsync(kernel, argument)).GetValue<object>())
                        : "Unable to find function. Please try again!";

                    chatHistory.Add(new ChatMessageContent(AuthorRole.Tool, content, metadata: new Dictionary<string, object?>(1)
                    {
                        { OpenAIChatMessageContent.ToolIdProperty, toolCall.Id }
                    }));
                }
            }

            WriteLine();
        }
        WriteLine("======== Example 4: Use automated function calling with a streaming chat ========");
    }
}
