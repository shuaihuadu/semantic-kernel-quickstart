namespace KernelSyntaxExamples.OwnerExamples;

public class Example006_TokenCalculator(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public void Run()
    {
        string result = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "OwnerExamples", "result.txt"));

        //List<Dictionary<string, string>> exampleMessages =
        //[
        //    new() {
        //        {"role", "user"},
        //        {"content", result}
        //    }
        //];

        var exampleMessages = new List<Dictionary<string, string>>
        {
            new()
            {
                {"role", "system"},
                {"content", "You are a helpful, pattern-following assistant that translates corporate jargon into plain English."},
            },
            new()
            {
                {"role", "system"},
                {"name", "example_user"},
                {"content", "New synergies will help drive top-line growth."},
            },
            new()
            {
                {"role", "system"},
                {"name", "example_assistant"},
                {"content", "Things working well together will increase revenue."},
            },
            new()
            {
                {"role", "system"},
                {"name", "example_user"},
                {"content", "Let's circle back when we have more bandwidth to touch base on opportunities for increased leverage."},
            },
            new()
            {
                {"role", "system"},
                {"name", "example_assistant"},
                {"content", "Let's talk later when we're less busy about how to do better."},
            },
            new()
            {
                {"role", "user"},
                {"content", "This late pivot means we don't have time to boil the ocean for the client deliverable."},
            },
        };

        WriteLine(NumTokensFromMessages(exampleMessages, "gpt-3.5-turbo-0613"));
        WriteLine(NumTokensFromMessages(exampleMessages, "gpt-4-0613"));
    }

    public int NumTokensFromMessages(List<Dictionary<string, string>> messages, string model)
    {
        int tokensPerMessage = 3;
        int tokensPerName = 1;

        GptEncoding encoding = GptEncoding.GetEncoding("cl100k_base");

        HashSet<string> modelSet =
        [
            "gpt-3.5-turbo-0613",
            "gpt-3.5-turbo-16k-0613",
            "gpt-4-0314",
            "gpt-4-32k-0314",
            "gpt-4-0613",
            "gpt-4-32k-0613",
        ];

        if (modelSet.Contains(model))
        {
            tokensPerMessage = 3;
            tokensPerName = 1;
        }

        if (model == "gpt-3.5-turbo-0301")
        {
            tokensPerMessage = 4;
            tokensPerName = -1;
        }

        if (model.Contains("gpt-3.5-turbo"))
        {
            WriteLine("Warning: gpt-3.5-turbo may update over time. Returning num tokens assuming gpt-3.5-turbo-0613.");
        }
        else if (model.Contains("gpt-4"))
        {
            WriteLine("Warning: gpt-4 may update over time. Returning num tokens assuming gpt-4-0613.");
        }
        else
        {
            throw new NotImplementedException($"num_tokens_from_messages() is not implemented for model {model}. See https://github.com/openai/openai-python/blob/main/chatml.md for information on how messages are converted to tokens.");
        }

        int numTokens = 0;

        foreach (var message in messages)
        {
            numTokens += tokensPerMessage;

            foreach (var item in message)
            {
                numTokens += encoding.Encode(item.Value).Count;

                if (item.Key == "name")
                {
                    numTokens += tokensPerName;
                }
            }
        }

        numTokens += 3;
        return numTokens;
    }
}
