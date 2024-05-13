namespace AutoFunctionCalling;

public class OpenAI_FunctionCalling(ITestOutputHelper output) : BaseTest(output)
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

        //await RunExample1Async(kernel);
        //await RunExample2Async(kernel);
        //await RunExample3Async(kernel);
        await RunExample4Async(kernel);
    }

    private async Task RunExample1Async(Kernel kernel)
    {

        Console.WriteLine("======== Example 1: Use automated function calling with a non-streaming prompt ========");
        {
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            Console.WriteLine(await kernel.InvokePromptAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(settings)));

            Console.WriteLine();
        }
    }

    private async Task RunExample2Async(Kernel kernel)
    {
        Console.WriteLine("======== Example 2: Use automated function calling with a streaming prompt ========");
        {
            OpenAIPromptExecutionSettings setting = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            await foreach (var update in kernel.InvokePromptStreamingAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(setting)))
            {
                Console.Write(update);
            }

            Console.WriteLine();
        }
    }

    private async Task RunExample3Async(Kernel kernel)
    {
        Console.WriteLine("======== Example 3: Use manual function calling with a non-streaming prompt ========");
        {
            IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = [];

            OpenAIPromptExecutionSettings setting = new()
            {
                ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
            };

            chatHistory.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

            while (true)
            {
                ChatMessageContent result = await chat.GetChatMessageContentAsync(chatHistory, setting, kernel);

                if (result.Content is not null)
                {
                    Console.Write(result.Content);
                }

                IEnumerable<FunctionCallContent> functionCalls = FunctionCallContent.GetFunctionCalls(result);

                if (!functionCalls.Any())
                {
                    break;
                }

                chatHistory.Add(result);

                foreach (var functionCall in functionCalls)
                {
                    try
                    {
                        FunctionResultContent resultContent = await functionCall.InvokeAsync(kernel);

                        chatHistory.Add(resultContent.ToChatMessage());
                    }
                    catch (Exception ex)
                    {
                        chatHistory.Add(new FunctionResultContent(functionCall, ex).ToChatMessage());
                    }
                }
            }

            Console.WriteLine();
        }
    }

    private async Task RunExample4Async(Kernel kernel)
    {
        Console.WriteLine("======== Example 4: Simulated function calling with a non-streaming prompt ========");
        {
            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
            };

            ChatHistory chatHistory = [];

            chatHistory.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

            while (true)
            {
                ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);

                if (result.Content is not null)
                {
                    Console.Write(result.Content);
                }

                chatHistory.Add(result);

                IEnumerable<FunctionCallContent> functionCalls = FunctionCallContent.GetFunctionCalls(result);

                if (!functionCalls.Any())
                {
                    break;
                }

                foreach (FunctionCallContent functionCall in functionCalls)
                {
                    FunctionResultContent resultContent = await functionCall.InvokeAsync(kernel);

                    chatHistory.Add(resultContent.ToChatMessage());
                }

                FunctionCallContent simulatedFunctionCall = new("weather-alter", id: "call_123");

                result.Items.Add(simulatedFunctionCall);

                string simluatedFunctionResult = "A Tornado Watch has been issued, with potential for severe thunderstorms causing unusual sky colors like green, yellow, or dark gray. Stay informed and follow safety instructions from authorities.";

                chatHistory.Add(new FunctionResultContent(simulatedFunctionCall, simluatedFunctionResult).ToChatMessage());
            }
        }
    }
}
