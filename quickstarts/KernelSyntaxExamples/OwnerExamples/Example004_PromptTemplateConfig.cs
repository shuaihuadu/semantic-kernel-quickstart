namespace KernelSyntaxExamples.OwnerExamples;

public class Example004_PromptTemplateConfig(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        string skill = "Translate";

        string configFileContent = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OwnerExamples", "Skills", "Default", skill, "config.json"));

        string promptContent = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OwnerExamples", "Skills", "Default", skill, "skprompt.txt"));

        PromptTemplateConfig promptTemplateConfig = PromptTemplateConfig.FromJson(configFileContent);

        Assert.NotNull(promptTemplateConfig);

        Assert.NotNull(promptTemplateConfig.DefaultExecutionSettings);

        Assert.NotNull(promptTemplateConfig.DefaultExecutionSettings.ExtensionData);

        double temperature = Convert.ToDouble(promptTemplateConfig.DefaultExecutionSettings.ExtensionData!["temperature"].ToString());

        WriteLine(temperature);
    }
}