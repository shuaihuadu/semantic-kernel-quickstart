using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Concepts;

[TestClass]
public class AzureOpenAI_ChatCompletion : BaseTest
{
    [TestMethod]
    public async Task ChatPromptAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey
            ).Build();
    }

    [TestMethod]
    public async Task ServicePromptAsync()
    {
        Console.WriteLine("======== Azure AI Inference - Chat Completion ========");

        IChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        await StartChatAsync(chatCompletionService);
    }

    private async Task StartChatAsync(IChatCompletionService chatCompletionService)
    {
        Console.WriteLine("Chat content:");
        Console.WriteLine("------------------------");

        ChatHistory chatHistory = [];

        ChatMessageContentItemCollection messages = [];

        string imageDataUri = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGAAAAAwCAMAAADZyI/9AAAAYFBMVEUAAAD/bRn/bhn/axj/bRj/bhn/cBP/bhn/bhj/bhj/bxj/bBb/bhn/////klP/toz/gDX/7eL/pG//dyf/9vH/rX3/28X/v5r/klL/5NT/iUT/yan/m2H/0rf/0bf/7eO/RNjOAAAADHRSTlMAf+8gX98Qz7+nj1AhG0XCAAACS0lEQVRYw7WXixKqIBCGEe+2loWS2eW8/1se2NAVKDlnkm+mUvvZb1hwHJlHkTV1zuH/yasmS1mAIqngJ8pNR5Fw+J36q4LK/0jCPpGWsBv5h0lkHHaEH7z2wM4kXv2ohgNEIFutL4cIcFrpHKJQ+gsQZxlSILqjAmaEPhOgONq8g9MqBob3CAMvUFADcWoVlyWtzzpQtBZngAF/3gyYQqZlBE0hBVfQd2GBWFWS6nAg1w3AmkLmCVoZEthVcQi5rs4qlK6ARtqC52lmMElJqZ5cIxAVdYgE1CRbcLGSo44JUNzbpezRTI8oWOYKEBkUQD9XvbWa6+y6O7dz4wl6PfQUFNxN1a5FbrPrYaUaVnmC89AqppDgoqPzr/oI4xJWqma5LwCpv4UrOB0N+gpt1Kea8wM3BrokWOQMPgi6Xk/fERAna/ur7EvLXsqFVyz4RwFgk8ZtAYYkZi5a1isXxm0+C0yTtgUdbtSrjqBspE0aFghski24XwyTCf/RVW+4gSYdv+rQPwrggXvb2UUO+u+n2ay98tzoNg4ssgaLbwtGkxgp3oND9VUgzkGB6Kno4x1+gUPzVQBjUADSFCWZl8m+CKhJ/o1Gz69hVVSasEO6IRDnlcBmTrwDJJP+c39DAFNIAJIeYB1O0uvQpgCG7wKKX1d3xeR1yBWM+LwigzoVWMlmmaM6ntv+WK4TNSNBFNLIgoTFFeQsroCnkQUHFleQsLiChEUV8IxFFZQpi/l2w5Oo75c8KdiaooQ9qZbyZEj26RLP6yYrmMtffiXQ5PJHSS0AAAAASUVORK5CYII=";

        messages.Add(new TextContent("图片里面有什么？"));
        messages.Add(new ImageContent(imageDataUri));

        chatHistory.AddUserMessage(messages);

        ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        chatHistory.Add(reply);

        OutputLastMessage(chatHistory);
    }
}
