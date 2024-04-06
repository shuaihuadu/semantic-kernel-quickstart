namespace DocumentationExamples.Plugins;

public class MathSolver
{
    private readonly ILogger _logger;

    public MathSolver(ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<MathSolver>();
    }


    public async Task<string> SolveAsync(Kernel kernel,
        [Description("The math problem to solve; describe it in 2-3 sentences to ensure full context is provided")] string problem)
    {
        Kernel kernelWithMath = kernel.Clone();

        kernelWithMath.Plugins.Remove(kernelWithMath.Plugins["MathSolver"]);

        kernelWithMath.Plugins.AddFromType<MathPlugin>();

        return string.Empty;
    }
}
