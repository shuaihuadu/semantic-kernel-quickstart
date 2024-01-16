namespace NCalcPlugins;

public class LanguageCalculatorPlugin
{
    private readonly KernelFunction _mathTranslator;
    private const string MathTranslatorPrompt =
        @"Translate a math problem into a expression that can be executed using .net NCalc library. Use the output of running this code to answer the question.
Available functions: Abs, Acos, Asin, Atan, Ceiling, Cos, Exp, Floor, IEEERemainder, Log, Log10, Max, Min, Pow, Round, Sign, Sin, Sqrt, Tan, and Truncate. in and if are also supported.

Question: $((Question with math problem.))
expression:``` $((single line mathematical expression that solves the problem))```

[Examples]
Question: What is 37593 * 67?
expression:```37593 * 67```

Question: what is 3 to the 2nd power?
expression:```Pow(3, 2)```

Question: what is sine of 0 radians?
expression:```Sin(0)```

Question: what is sine of 45 degrees?
expression:```Sin(45 * Pi /180 )```

Question: how many radians is 45 degrees?
expression:``` 45 * Pi / 180 ```

Question: what is the square root of 81?
expression:```Sqrt(81)```

Question: what is the angle whose sine is the number 1?
expression:```Asin(1)```

[End of Examples]

Question: {{ $input }}
";

    public LanguageCalculatorPlugin()
    {
        this._mathTranslator = KernelFunctionFactory.CreateFromPrompt(
            MathTranslatorPrompt,
            functionName: "TranslateMathProblem",
            description: "Used by 'Calculator' function.",
            executionSettings: new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    {"MaxTokens",256 },
                    {"Temperature",0.0 },
                    {"TopP",1 }
                }
            });
    }

    [KernelFunction("Calculator")]
    [Description("Useful for getting the result of a non-trivial math expression.")]
    public async Task<string> CalculateAsync(
        [Description("A valid mathematical expression that could be executed by a calculator capable of more advanced math functions like sin/cosine/floor.")] string input,
        Kernel kernel)
    {
        string answer;

        try
        {
            FunctionResult result = await kernel.InvokeAsync(this._mathTranslator, new() { ["input"] = input }).ConfigureAwait(false);

            answer = result?.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error in calculator for input {input} {ex.Message}", ex);
        }

        string pattern = @"```\s*(.*?)\s*```";

        Match match = Regex.Match(answer, pattern, RegexOptions.Singleline);

        if (match.Success)
        {
            string result = EvaluateMathExpression(match);

            return result;
        }

        throw new InvalidOperationException($"Input value [{input}] could not be understood, received following {answer}");
    }

    private static string EvaluateMathExpression(Match match)
    {
        string textExpressions = match.Groups[1].Value;

        Expression expression = new(textExpressions, EvaluateOptions.IgnoreCase);

        expression.EvaluateParameter += (string name, ParameterArgs args) =>
        {
            args.Result = name.ToLower(CultureInfo.CurrentCulture) switch
            {
                "pi" => Math.PI,
                "e" => Math.E,
                _ => args.Result
            };
        };

        try
        {
            if (expression.HasErrors())
            {
                return "Error: " + expression.Error + " could not evaluate " + textExpressions;
            }

            object result = expression.Evaluate();

            return "Answer:" + result.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("could not evaluate " + textExpressions, ex);
        }
    }
}
