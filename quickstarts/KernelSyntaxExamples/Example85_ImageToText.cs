namespace KernelSyntaxExamples;

public class Example85_ImageToText(ITestOutputHelper output) : BaseTest(output)
{
    private const string ImageToTextModel = "Salesforce/blip-image-captioning-base";
    private const string ImageFilePath = "test_image.jpg";

    [Fact(Skip = "TODO Hugging Face ")]
    public async Task ImageToTextAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddHuggingFaceImageToText(
                model: ImageToTextModel,
                apiKey: TestConfiguration.HuggingFace.ApiKey)
            .Build();


        IImageToTextService imageToText = kernel.GetRequiredService<IImageToTextService>();

        HuggingFacePromptExecutionSettings executionSettings = new()
        {
            MaxTokens = 500
        };

        ReadOnlyMemory<byte> imageData = await EmbeddedResource.ReadAllAsync(ImageFilePath);

        ImageContent imageContent = new(new BinaryData(imageData, "image/jpeg"));

        TextContent textContent = await imageToText.GetTextContentAsync(imageContent, executionSettings);

        WriteLine(textContent.Text);
    }
}
