namespace DocumentationExamples.Plugins;

public class MathSolver(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [KernelFunction]
    [Description("Solves a math problem.")]
    [return: Description("The solution to the math problem.")]
    public async Task<string> SolveAsync(Kernel kernel, [Description("The math problem to solve; describe it in 2-3 sentences to ensure full context is provided")] string problem)
    {
        Kernel kernelWithMath = kernel.Clone();

        kernelWithMath.Plugins.Remove(kernelWithMath.Plugins["MathSolver"]);

        kernelWithMath.Plugins.AddFromType<MathPlugin>();

        HandlebarsPlanner planner = new(new HandlebarsPlannerOptions
        {
            AllowLoops = true
        });

        HandlebarsPlan plan = await planner.CreatePlanAsync(kernelWithMath, problem);

        this._output.WriteLine("Plan: {Plan}", plan);

        string result = (await plan.InvokeAsync(kernelWithMath)).Trim();

        this._output.WriteLine("Results: {Result}", result);

        return result;
    }
}
