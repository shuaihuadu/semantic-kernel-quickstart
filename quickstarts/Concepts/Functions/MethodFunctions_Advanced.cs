namespace Functions;

public class MethodFunctions_Advanced(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task MethodFunctionsChainingAsync()
    {
        this.WriteLine("Running Method Function Chaining example...");

        Kernel kernel = new();

        KernelPlugin functions = kernel.ImportPluginFromType<FunctionsChainingPlugin>();

        CustomType? customType = await kernel.InvokeAsync<CustomType>(functions["Function1"]);

        this.WriteLine($"CustomType.Number: {customType!.Number}");
        this.WriteLine($"CustomType.Text: {customType.Text}");
    }

    private sealed class FunctionsChainingPlugin
    {
        public const string PluginName = nameof(FunctionsChainingPlugin);

        [KernelFunction]
        public async Task<CustomType> Function1Async(Kernel kernel)
        {
            CustomType? result = await kernel.InvokeAsync<CustomType>(PluginName, "Function2");

            return new CustomType
            {
                Number = 2 * result?.Number ?? 0,
                Text = "From Function1 + " + result?.Text
            };
        }

        [KernelFunction]
        public static CustomType Function2()
        {
            return new CustomType
            {
                Number = 2,
                Text = "From Function2"
            };
        }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    private sealed class CustomType
    {
        public int Number { get; set; }

        public string? Text { get; set; }
    }

    private sealed class CustomTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => true;

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            return JsonSerializer.Deserialize<CustomType>((string)value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}