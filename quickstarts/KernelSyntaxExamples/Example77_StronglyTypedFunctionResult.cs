
namespace KernelSyntaxExamples;

public class Example77_StronglyTypedFunctionResult : BaseTest
{
    [Fact]
    public async Task RunAsync()
    {
        this.WriteLine("======== Extended function result ========");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        string promptTestDataGeneration = "Return a JSON with an array of 3 JSON objects with the following fields: "
            + "First, an id field with a random GUID, next a name field with a random company name and last a description field with a random short company description. "
            + "Ensure the JSON is valid and it contains a JSON array named testcompanies with the three fields.";

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        FunctionResult functionResult = await kernel.InvokePromptAsync(promptTestDataGeneration);

        stopwatch.Stop();

        FunctionResultTestDataGen functionResultTestDataGen = new FunctionResultTestDataGen(functionResult, stopwatch.ElapsedMilliseconds);

        this.WriteLine($"Test data: {functionResultTestDataGen.Result} \n");
        this.WriteLine($"Milliseconds: {functionResultTestDataGen.ExecutionTimeInMilliseconds} \n");
        this.WriteLine($"Total Tokens:{functionResultTestDataGen.TokenCounts!.TotalTokens} \n");
    }

    public Example77_StronglyTypedFunctionResult(ITestOutputHelper output) : base(output)
    {
    }

    private sealed class RootObject
    {
        public List<TestCompany> TestCompanies { get; set; } = new List<TestCompany>();
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
            RootObject? rootObject = JsonSerializer.Deserialize<RootObject>(this.Result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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

    private sealed class TokenCounts
    {
        public int CompletionTokens { get; set; }

        public int PromptTokens { get; set; }

        public int TotalTokens { get; set; }

        public TokenCounts(int completionTokens, int promptTokens, int totalTokens)
        {
            this.CompletionTokens = completionTokens;
            this.PromptTokens = promptTokens;
            this.TotalTokens = totalTokens;
        }
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
