namespace Functions;

public class FunctionResult_StronglyTyped(ITestOutputHelper output) : BaseTest(output)
{
    //[Fact(Skip = "Prompt Optimize")]
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Extended function result ========");

        Kernel kernel = KernelHelper.AzureOpenAIChatCompletionKernelBuilder().Build();

        string promptTestDataGeneration = "Return a JSON with an array of 3 JSON objects with the following fields: "
            + "First, an id field with a random GUID, next a name field with a random company name and last a description field with a random short company description. "
            + "Ensure the JSON is valid and it contains a JSON array named testcompanies with the three fields."
            + "ONLY output the JSON content, DO NOT add any other texts and markdown Code block!";

        Stopwatch stopwatch = new();
        stopwatch.Start();

        FunctionResult functionResult = await kernel.InvokePromptAsync(promptTestDataGeneration);

        stopwatch.Stop();

        FunctionResultTestDataGen functionResultTestDataGen = new(functionResult, stopwatch.ElapsedMilliseconds);

        Console.WriteLine($"Test data: {functionResultTestDataGen.Result} \n");
        Console.WriteLine($"Milliseconds: {functionResultTestDataGen.ExecutionTimeInMilliseconds} \n");
        Console.WriteLine($"Total Tokens:{functionResultTestDataGen.TokenCounts!.TotalTokens} \n");
    }

    private sealed class RootObject
    {
        public List<TestCompany> TestCompanies { get; set; } = [];
    }

    public sealed class TestCompany
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    private sealed class FunctionResultTestDataGen : FunctionResultExtended
    {
        public List<TestCompany> TestCompanies { get; set; } = [];

        public long ExecutionTimeInMilliseconds { get; init; }

        public FunctionResultTestDataGen(FunctionResult functionResult, long executionTimeInMilliseconds) :
            base(functionResult)
        {
            this.TestCompanies = this.ParaseTestCompanies();
            this.ExecutionTimeInMilliseconds = executionTimeInMilliseconds;
            this.TokenCounts = this.ParseTokenCounts();
        }

        private List<TestCompany> ParaseTestCompanies()
        {
            RootObject? rootObject = JsonSerializer.Deserialize<RootObject>(this.Result);

            List<TestCompany> companies = rootObject!.TestCompanies;

            return companies;
        }

        private TokenCounts? ParseTokenCounts()
        {
            CompletionsUsage? usage = FunctionResult.Metadata?["Usage"] as CompletionsUsage;

            return new TokenCounts(
                completionTokens: usage?.CompletionTokens ?? 0,
                promptTokens: usage?.PromptTokens ?? 0,
                totalTokens: usage?.TotalTokens ?? 0);
        }
    }

    private sealed class TokenCounts(int completionTokens, int promptTokens, int totalTokens)
    {
        public int CompletionTokens { get; set; } = completionTokens;

        public int PromptTokens { get; set; } = promptTokens;

        public int TotalTokens { get; set; } = totalTokens;
    }

    private class FunctionResultExtended
    {
        public string Result { get; set; } = string.Empty;

        public TokenCounts? TokenCounts { get; set; }

        public FunctionResult FunctionResult { get; init; }

        public FunctionResultExtended(FunctionResult functionResult)
        {
            this.FunctionResult = functionResult;
            this.Result = ParseResultFromFunctionResult();
        }

        private string ParseResultFromFunctionResult()
        {
            return this.FunctionResult.GetValue<string>() ?? string.Empty;
        }
    }
}
