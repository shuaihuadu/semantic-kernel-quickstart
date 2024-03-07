
namespace KernelSyntaxExamples;

public class Example002_ActualTokenCalculate(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public void Run()
    {
        List<Dictionary<string, string>> exampleMessages =
        [
            new() {
                {"role", "user"},
                {"content", "请帮我回答一下什么是OpenAI"}
            }
        ];

        int count = this.CalculateTokensCountForMessage(exampleMessages);

        WriteLine(count);
    }


    public int CalculateTokensCountForMessage(List<Dictionary<string, string>> messages)
    {
        int tokensPerMessage = 3;
        int tokensPerName = 1;
        GptEncoding encoding = GptEncoding.GetEncoding("cl100k_base");

        //if (model == "gpt-3.5-turbo-0301")
        //tokensPerMessage = 4;
        //tokensPerName = -1;

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

        numTokens += 3;

        return numTokens;
    }
}