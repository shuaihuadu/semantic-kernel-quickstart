namespace KernelSyntaxExamples.GettingStart;

public class Step8_Pipelining : BaseTest
{

    [Fact]
    public async Task RunAsync()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
            endpoint: TestConfiguration.AzureOpenAI.Endpoint,
            apiKey: TestConfiguration.AzureOpenAI.ApiKey);

        kernelBuilder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));

        Kernel kernel = kernelBuilder.Build();

        WriteLine("================ PIPELINE ================");
        {
            // Create a pipeline of functions that will parse a string into an int, multiply it by a double, truncate it to an int, and then humanize it.
            KernelFunction parseInt32 = KernelFunctionFactory.CreateFromMethod((string s) => double.Parse(s, CultureInfo.InvariantCulture), "parseInt32");
            KernelFunction multiplyByN = KernelFunctionFactory.CreateFromMethod((double i, double n) => i * n, "multiplyByN");
            KernelFunction truncate = KernelFunctionFactory.CreateFromMethod((double d) => (int)d, "truncate");
            KernelFunction humanize = KernelFunctionFactory.CreateFromPrompt(new PromptTemplateConfig
            {
                Template = "Spell out this number in English: {{$number}}",
                InputVariables = new() { new() { Name = "number" } }
            });

            KernelFunction pipeline = KernelFunctionCombinators.Pipe(new[] { parseInt32, multiplyByN, truncate, humanize }, "pipeline");

            KernelArguments arguments = new()
            {
                ["s"] = "10",
                ["n"] = (double)78.9
            };

            FunctionResult functionResult = await pipeline.InvokeAsync(kernel, arguments);

            WriteLine(functionResult.ToString());
        }

        WriteLine("================ GRAPH ================");
        {
            KernelFunction rand = KernelFunctionFactory.CreateFromMethod(() => Random.Shared.Next(), "GetRandomInt32");
            KernelFunction mult = KernelFunctionFactory.CreateFromMethod((int i, int j) => i * j, "Multiply");

            KernelFunction graph = KernelFunctionCombinators.Pipe(new[]
            {
                (rand,"i"),
                (rand,"j"),
                (mult,"")
            }, "graph");

            FunctionResult functionResult = await graph.InvokeAsync(kernel);

            WriteLine(functionResult.ToString());
        }
    }

    public Step8_Pipelining(ITestOutputHelper output) : base(output)
    {
    }
}

public static class KernelFunctionCombinators
{
    public static Task<FunctionResult> InvokePipelineAsync(
        IEnumerable<KernelFunction> functions,
        Kernel kernel,
        KernelArguments arguments,
        CancellationToken cancellationToken)
        => Pipe(functions).InvokeAsync(kernel, arguments, cancellationToken);

    public static Task<FunctionResult> InvokePipelineAsync(
        IEnumerable<(KernelFunction Function, string OutputVariable)> functions,
        Kernel kernel,
        KernelArguments arguments,
        CancellationToken cancellationToken)
        => Pipe(functions).InvokeAsync(kernel, arguments, cancellationToken);


    public static KernelFunction Pipe(
        IEnumerable<KernelFunction> functions,
        string? functionName = null,
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(functions, nameof(functions));

        KernelFunction[] kernelFunctions = functions.ToArray();

        Array.ForEach(kernelFunctions, f => ArgumentNullException.ThrowIfNull(f));

        (KernelFunction Function, string OutputVariable)[] funcsAndVars = new (KernelFunction Function, string OutputVariable)[kernelFunctions.Length];

        for (int i = 0; i < kernelFunctions.Length; i++)
        {
            string p = string.Empty;

            if (i < kernelFunctions.Length - 1)
            {
                IReadOnlyList<KernelParameterMetadata> kernelParameters = kernelFunctions[i + 1].Metadata.Parameters;

                if (kernelParameters.Count > 0)
                {
                    p = kernelParameters[0].Name;
                }
            }

            funcsAndVars[i] = (kernelFunctions[i], p);
        }

        return Pipe(funcsAndVars, functionName, description);
    }

    public static KernelFunction Pipe(
        IEnumerable<(KernelFunction Function, string OutputVariable)> functions,
        string? functionName = null,
        string? description = null)
    {
        ArgumentNullException.ThrowIfNull(functions);

        (KernelFunction Function, string OutputVariable)[] arr = functions.ToArray();

        Array.ForEach(arr, f =>
        {
            ArgumentNullException.ThrowIfNull(f.Function);
            ArgumentNullException.ThrowIfNull(f.OutputVariable);
        });

        return KernelFunctionFactory.CreateFromMethod(async (Kernel kernel, KernelArguments arguments) =>
        {
            FunctionResult? result = null;

            for (int i = 0; i < arr.Length; i++)
            {
                result = await arr[i].Function.InvokeAsync(kernel, arguments).ConfigureAwait(false);

                if (i < arr.Length - 1)
                {
                    arguments[arr[i].OutputVariable] = result.GetValue<object>();
                }
            }

            return result;

        }, functionName, description);
    }
}