using Microsoft.SemanticKernel.AudioToText;

namespace KernelSyntaxExamples;

public sealed class Example82_Audio : BaseTest
{
    private const string AudioFilePath = "audio.wav";

    [Fact]
    public async Task TextToAudioAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAITextToAudio(
            deploymentName: TestConfiguration.AzureOpenAI.TTSDeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.TTSEndpoint,
            apiKey: TestConfiguration.AzureOpenAI.TTSApiKey
            ).Build();

        ITextToAudioService textToAudioService = kernel.GetRequiredService<ITextToAudioService>();

        string sampleText = "Hello, my name is John. I am a software engineer. I am working on a project to convert text to audio";

        OpenAITextToAudioExecutionSettings settings = new("alloy")
        {
            Voice = "alloy",
            ResponseFormat = "mp3",
            Speed = 1.0f
        };

        AudioContent audioContent = await textToAudioService.GetAudioContentAsync(sampleText, settings);

        await File.WriteAllBytesAsync(AudioFilePath, audioContent.Data!.ToArray());
    }

    [Fact(Skip = "When Azure Open AI support speech to text")]
    public async Task AudioToTextAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIAudioToText(
                deploymentName: TestConfiguration.AzureOpenAI.TTSDeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.TTSEndpoint,
                apiKey: TestConfiguration.AzureOpenAI.TTSApiKey)
            .Build();


        IAudioToTextService audioToTextService = kernel.GetRequiredService<IAudioToTextService>();

        OpenAIAudioToTextExecutionSettings settings = new(AudioFilePath)
        {
            Language = "en",
            Prompt = "sample prompt",
            ResponseFormat = "json",
            Temperature = 0.3f
        };

        ReadOnlyMemory<byte> audioData = await File.ReadAllBytesAsync(AudioFilePath);
        AudioContent audioContent = new(new BinaryData(audioData));

        TextContent textContent = await audioToTextService.GetTextContentAsync(audioContent, settings);

        WriteLine(textContent);
    }

    public Example82_Audio(ITestOutputHelper output) : base(output)
    {
    }
}
