using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.Text;

namespace AITeacher.Demo;

class Program
{
    static async Task Main(string[] args)
    {
        var kernel = Kernel.Builder
            //.WithAzureTextCompletionService("text-davinci-003", "https://mcd.openai.azure.com/", "ebecc5a110134cec9ac9a50d98f77754")
            .WithAzureTextCompletionService("text-davinci-003", "https://aoai-kk-ue1-common-01.openai.azure.com/", "ca814d276b534e71ab114c5ef259363a")
            //.WithAzureChatCompletionService("hsbc-gpt-35", "https://aoai-kk-ue1-common-01.openai.azure.com/", "ca814d276b534e71ab114c5ef259363a")
            .Build();

        var context = new ContextVariables();
        var histories = new StringBuilder();

        var skill = kernel.ImportSemanticSkillFromDirectory("Skills", "Learning");

        Console.WriteLine("请输入任何文字进行开始");

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            var input = Console.ReadLine();

            context.Set("history", histories.ToString());

            context.Set("input", input);

            var result = await kernel.RunAsync(context, skill["LearningEnglishSkill"]);

            histories.AppendLine(input);
            Console.ForegroundColor = ConsoleColor.Green;
            histories.AppendLine(result.Result.ToString());

            Console.WriteLine(result);
            Console.WriteLine();
        }
    }
}
