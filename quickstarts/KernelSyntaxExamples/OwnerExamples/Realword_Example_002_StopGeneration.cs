namespace KernelSyntaxExamples.OwnerExamples;

public class Realword_Example_002_StopGeneration(ITestOutputHelper output) : BaseTest(output)
{
    private static async Task StreamCompletionAsync(CancellationToken cancellationToken)
    {
        var url = "https://api.openai.com/v1/chat/completions";
        var apiKey = "YOUR_API_KEY";
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = "Hello, how are you?" } },
            stream = true
        };

        var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

        using var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("\nStreaming stopped by user.");
                break;
            }

            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrEmpty(line))
            {
                Console.Write(line);
            }
        }
    }

    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        var streamingTask = StreamCompletionAsync(cts.Token);

        Console.WriteLine("Press Enter to stop streaming...");
        Console.ReadLine();
        cts.Cancel();

        try
        {
            await streamingTask;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Streaming was cancelled.");
        }
    }
}