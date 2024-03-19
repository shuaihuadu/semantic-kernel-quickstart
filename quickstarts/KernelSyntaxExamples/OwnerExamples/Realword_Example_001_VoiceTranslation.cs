namespace KernelSyntaxExamples.OwnerExamples;

public class Realword_Example_001_VoiceTranslation(ITestOutputHelper output) : BaseTest(output)
{
    private const string AudioFileFolder = "chatcontent";

    private const string AudioFilePathPattern = "{0}\\{1}-{2}-audio.wav";

    [Fact]
    public async Task RunAsync()
    {
        if (!Directory.Exists(AudioFileFolder))
        {
            Directory.CreateDirectory(AudioFileFolder);
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAITextToAudio(
            deploymentName: TestConfiguration.AzureOpenAI.TTSDeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.TTSEndpoint,
            apiKey: TestConfiguration.AzureOpenAI.TTSApiKey
            ).Build();

        ITextToAudioService textToAudioService = kernel.GetRequiredService<ITextToAudioService>();

        string voice = "nova";

        OpenAITextToAudioExecutionSettings settings = new(voice)
        {
            Voice = voice,
            ResponseFormat = "mp3",
            Speed = 1.0f
        };

        int index = 1;

        foreach (ChatContent chatContent in ChatContent.ChatContents)
        {
            AudioContent audioContent = await textToAudioService.GetAudioContentAsync(chatContent.Content, settings);

            string audioFilePath = string.Format(AudioFilePathPattern, AudioFileFolder, chatContent.Name, index);

            await File.WriteAllBytesAsync(audioFilePath, audioContent.Data!.Value.ToArray());

            index++;

            await Task.Delay(60000);

            Thread.Sleep(60000);
        }
    }
}

public class ChatContent
{
    public ChatContent()
    {
    }

    public ChatContent(string name, string content)
    {
        Name = name;
        Content = content;
    }

    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public readonly static List<ChatContent> ChatContents =
    [
        new ChatContent("A","你好，希望一切顺利。我想问一下我们上周讨论的合同进展如何了，有什么更新吗？"),
        new ChatContent("B","你好！我很好，感谢你的问候。关于合同，我们目前处于最后审核阶段。预计本周末之前可以准备好供您审阅。"),
        new ChatContent("A","听到这个消息很高兴！与我们最初讨论的内容相比，会有什么重大变化吗？"),
        new ChatContent("B","没有太大的变化。只是对条款做了一些小的调整，以确保符合我们最新的政策。我会特别指出这些更改供您参考。"),
        new ChatContent("A","很好，我很感激。另外，我还想问一下我们上一个项目的退款进度如何了？"),
        new ChatContent("B","是的，我为您查看了一下。退款已经处理了，但似乎银行那边有些延迟。您应该能在接下来的3-5个工作日内看到账户里反映的金额。"),
        new ChatContent("A","明白了，感谢您的更新。退款正式发出后，您能否给我发送一封确认邮件？"),
        new ChatContent("B","当然可以，一旦完成我就立即发给您。我还会附上交易参考号供您记录。"),
        new ChatContent("A","那太好了，谢谢。既然我们谈到了这个话题，下周能否安排一个简短的会议来讨论未来的项目？"),
        new ChatContent("B","当然，我认为这是个好主意。您对日期和时间有什么偏好吗？"),
        new ChatContent("A","周三上午10点怎么样？这样我们有足够的时间来审阅合同并讨论新的机会。"),
        new ChatContent("B","周三上午10点对我来说没问题。我一会就给您发送日历邀请。"),
        new ChatContent("A","太好了，期待我们的会议。感谢您今天的帮助。"),
        new ChatContent("B","不客气！到时候再聊。")
    ];
}

public class ChatLanguageContent : ChatContent
{
    public string Language { get; set; } = string.Empty;
}

public class VoiceChatContent : ChatLanguageContent
{
    public BinaryData Voice { get; set; } = null!;
}