namespace NCalcPlugins;

public class SimpleCalculatorPlugin
{
    private readonly KernelFunction _mathTranslator;

    private static readonly string[] stopSequences = ["Problem:", "Solution:"];

    public SimpleCalculatorPlugin()
    {
        this._mathTranslator = KernelFunctionFactory.CreateFromPrompt(
            "Task: Give the final solution for the problem. Be as concise as possible.\nProblem:4+4\nSolution:8\nProblem:{{$input}}\nSolution:\n",
            functionName: "Calculator",
            description: "Evaluate a mathematical expression. Input is a valid mathematical expression that could be executed by a simple calculator i.e. add, subtract, multiply and divide. Cannot use variables.",
            executionSettings: new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    {"MaxTokens",256 },
                    {"Temperature",0.0 },
                    {"StopSequences",stopSequences }
                }
            });
    }
}
