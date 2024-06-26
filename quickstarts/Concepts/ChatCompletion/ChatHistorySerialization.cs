﻿namespace ChatCompletion;

public class ChatHistorySerialization(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public void SerializeChatHistoryWithSKContentType()
    {
        int[] data = [1, 2, 3];

        ChatMessageContent message = new(AuthorRole.User, "Describe the factors contributing to climate change.")
        {
            Items =
            [
                new TextContent("Discuss the potential long-term consequences for the Earth's ecosystem as well."),
                new ImageContent(new Uri("https://fake-random-test-host:123")),
                new BinaryContent(new BinaryData(data)),
                new AudioContent(new BinaryData(data))
            ]
        };

        ChatHistory chatHistory = new(new[] { message });

        string chatHistoryJson = JsonSerializer.Serialize(chatHistory);

        ChatHistory? deserializedHistory = JsonSerializer.Deserialize<ChatHistory>(chatHistoryJson);

        ChatMessageContent? deserializedMessage = deserializedHistory!.Single();

        Console.WriteLine($"Content: {deserializedMessage.Content}");

        Console.WriteLine($"Role: {deserializedMessage.Role.Label}");

        Console.WriteLine($"Text content: {(deserializedMessage.Items![0]! as TextContent)!.Text}");

        Console.WriteLine($"Image content: {(deserializedMessage.Items![1]! as ImageContent)!.Uri}");

        Console.WriteLine($"Binary content: {(deserializedMessage.Items![2]! as BinaryContent)!.Content}");

        Console.WriteLine($"Audio content: {(deserializedMessage.Items![3]! as AudioContent)!.Data}");
    }

    [Fact]
    public void SerializeChatWithHistoryWithCustomContentType()
    {
        ChatMessageContent message = new(AuthorRole.User, "Describe the factors contributing to climate change.")
        {
            Items =
            [
                new TextContent("Discuss the potential long-term consequences for the Earth's ecosystem as well."),
                new CustomContent("Some custom content")
            ]
        };

        ChatHistory chatHistory = new(new[] { message });

        JsonSerializerOptions options = new()
        {
            TypeInfoResolver = new CustomResolver()
        };

        string chatHistoryJson = JsonSerializer.Serialize(chatHistory, options);

        ChatHistory? deserializedHistory = JsonSerializer.Deserialize<ChatHistory>(chatHistoryJson, options);

        ChatMessageContent? deserializedMessage = deserializedHistory!.Single();

        Console.WriteLine($"Content: {deserializedMessage.Content}");

        Console.WriteLine($"Role: {deserializedMessage.Role.Label}");

        Console.WriteLine($"Text content: {(deserializedMessage.Items![0]! as TextContent)!.Text}");

        Console.WriteLine($"Custom content: {(deserializedMessage.Items![1]! as CustomContent)!.Content}");
    }

    private sealed class CustomContent(string content) : KernelContent(content)
    {
        public string Content { get; } = content;
    }

    private sealed class CustomResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            if (jsonTypeInfo.Type != typeof(KernelContent))
            {
                return jsonTypeInfo;
            }

            jsonTypeInfo.PolymorphismOptions ??= new JsonPolymorphismOptions();

            jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(typeof(CustomContent), "customContent"));

            jsonTypeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName = typeof(CustomContent).Name;

            return jsonTypeInfo;
        }
    }
}
