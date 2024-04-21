namespace KernelSyntaxExamples;

public class Example69_MutableKernelPlugin(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        MutableKernelPlugin plugin = new("Plugin");

        KernelFunction function = KernelFunctionFactory.CreateFromMethod(() => "This is result from Function", "Function");
        KernelFunction dateFunction = KernelFunctionFactory.CreateFromMethod((string? format = null) => DateTime.Now.ToString(format, CultureInfo.InvariantCulture), "Date", "Plugin.Date");

        plugin.AddFunction(function);
        plugin.AddFunction(dateFunction);

        Kernel kernel = new();
        kernel.Plugins.Add(plugin);

        FunctionResult result = await kernel.InvokeAsync(kernel.Plugins["Plugin"]["Function"]);

        this.WriteLine($"Result: {result.GetValue<string>()}");

        result = await kernel.InvokeAsync(kernel.Plugins["Plugin"]["Date"], new()
        {
            ["format"] = "yyyy/MM/dd HH:mm:ss"
        });

        this.WriteLine($"Result: {result.GetValue<string>()}");
    }

    public class MutableKernelPlugin : KernelPlugin
    {
        private readonly Dictionary<string, KernelFunction> _functions;

        public MutableKernelPlugin(string name, string? description = null, IEnumerable<KernelFunction>? functions = null) : base(name, description)
        {
            this._functions = new Dictionary<string, KernelFunction>(StringComparer.OrdinalIgnoreCase);

            if (functions is not null)
            {
                foreach (KernelFunction function in functions)
                {
                    ArgumentNullException.ThrowIfNull(function);

                    KernelFunction cloned = function.Clone(name);

                    this._functions.Add(cloned.Name, cloned);
                }
            }
        }

        public override int FunctionCount => this._functions.Count;

        public override IEnumerator<KernelFunction> GetEnumerator() => this._functions.Values.GetEnumerator();

        public override bool TryGetFunction(string name, [NotNullWhen(true)] out KernelFunction? function) => this._functions.TryGetValue(name, out function);

        public void AddFunction(KernelFunction function)
        {
            ArgumentNullException.ThrowIfNull(function);

            KernelFunction cloned = function.Clone(this.Name);

            this._functions.Add(cloned.Name, cloned);
        }
    }
}