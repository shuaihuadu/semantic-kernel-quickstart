namespace KernelSyntaxExamples.OwnerExamples;

public class Example002_ActualTokenCalculate(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public void Run()
    {
        List<Dictionary<string, string>> exampleMessages =
        [
            new() {
            {"role", "user"},
            {"content", "What is OpenAI?"}
        }
        ];

        int count = CalculateTokensCountForMessage(exampleMessages);

        WriteLine(count);
    }


    public int CalculateTokensCountForMessage(List<Dictionary<string, string>> messages)
    {
        int tokensPerMessage = 3;
        int tokensPerName = 1;
        GptEncoding encoding = GptEncoding.GetEncoding("cl100k_base");

        //if (model == "gpt-3.5-turbo-0301")
        //tokensPerMessage = 4; // every message follows <|start|>{role/name}\n{content}<|end|>\n
        //tokensPerName = -1; // if there's a name, the role is omitted

        int numTokens = 0;

        foreach (var message in messages)
        {
            numTokens += tokensPerMessage;

            foreach (var item in message)
            {
                List<int> tokens = encoding.Encode(item.Value);

                numTokens += tokens.Count;

                if (item.Key == "name")
                {
                    numTokens += tokensPerName;
                }
            }
        }

        numTokens += 3; //every reply is primed with <|start|>assistant<|message|>

        return numTokens;
    }
}