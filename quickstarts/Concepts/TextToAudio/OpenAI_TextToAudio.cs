namespace TextToAudio;

public class OpenAI_TextToAudio
{
    [Fact(Skip = "Uncomment the line to write the audio file output before running this test.")]
    public async Task TextToAudio()
    {
        Kernel kernel = Kernel.CreateBuilder().AddAzureOpenAITextToAudio(
            deploymentName: TestConfiguration.AzureOpenAITTS.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAITTS.Endpoint,
            apiKey: TestConfiguration.AzureOpenAITTS.ApiKey)
            .Build();

        ITextToAudioService textToAudioService = kernel.GetRequiredService<ITextToAudioService>();

        string sampleText = "Hello, my name is John. I am a software engineer. I am working on a project to convert text to audio.";

        OpenAITextToAudioExecutionSettings executionSettings = new()
        {
            Voice = "alloy",
            ResponseFormat = "mp3",
            Speed = 1.0f
        };

        AudioContent audioContent = await textToAudioService.GetAudioContentAsync(sampleText, executionSettings);

        //await File.WriteAllBytesAsync(AudioFilePath, audioContent.Data!.Value.ToArray());
    }
}
