﻿namespace AudioToText;

public sealed class OpenAI_AudioToText(ITestOutputHelper output) : BaseTest(output)
{
    private const string AudioFilePath = "test_audio.wav";

    [Fact]
    public async Task TextToAudioAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAITextToAudio(
            deploymentName: TestConfiguration.AzureOpenAITTS.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAITTS.Endpoint,
            apiKey: TestConfiguration.AzureOpenAITTS.ApiKey
            ).Build();

        ITextToAudioService textToAudioService = kernel.GetRequiredService<ITextToAudioService>();

        string sampleText = "公共安全行业标准《生物样品血液、尿液中乙醇、甲醇、正丙醇、乙醛、丙酮、异丙醇和正丁醇的顶空—气相色谱检验方法》等标准发布实施以来，为机动车驾驶人血液酒精含量检测提供了技术依据。\r\n\r\n全国刑事技术标准化技术委员会在进一步丰富优化该项行业标准检测分析方法、细化完善有关技术要求的基础上，组织制定了《血液、尿液中乙醇、甲醇、正丙醇、丙酮、异丙醇和正丁醇检验》（GB/T42430-2023）国家标准。";

        //Experiment with different voices (alloy, echo, fable, onyx, nova, and shimmer) 

        string voice = "nova";

        OpenAITextToAudioExecutionSettings settings = new(voice)
        {
            Voice = voice,
            ResponseFormat = "mp3",
            Speed = 1.0f
        };

        AudioContent audioContent = await textToAudioService.GetAudioContentAsync(sampleText, settings);

        await File.WriteAllBytesAsync(AudioFilePath, audioContent.Data!.Value.ToArray());
    }

    [Fact]
    public async Task AudioToTextAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIAudioToText(
                deploymentName: TestConfiguration.AzureOpenAIWhisper.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAIWhisper.Endpoint,
                apiKey: TestConfiguration.AzureOpenAIWhisper.ApiKey)
            .Build();


        IAudioToTextService audioToTextService = kernel.GetRequiredService<IAudioToTextService>();

        OpenAIAudioToTextExecutionSettings settings = new(AudioFilePath)
        {
            Language = "en",
            Prompt = "简体中文，正确使用标点符号",
            ResponseFormat = "json",
            Temperature = 0.3f
        };

        await using var audioFileStream = EmbeddedResource.ReadStream(AudioFilePath);

        var audioFileBinaryData = await BinaryData.FromStreamAsync(audioFileStream!);

        AudioContent audioContent = new(new BinaryData(audioFileBinaryData));

        TextContent textContent = await audioToTextService.GetTextContentAsync(audioContent, settings);

        Console.WriteLine(textContent);
    }
}
