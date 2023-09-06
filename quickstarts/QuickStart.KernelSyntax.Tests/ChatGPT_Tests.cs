namespace QuickStart.KernelSyntax.Tests
{
    [TestClass]
    public class ChatGPT_Tests : TestBase
    {
        private IKernel kernel;

        [TestInitialize]
        public void InitKernel()
        {
            kernel = Kernel.Builder
                .WithAzureTextEmbeddingGenerationService(QuickStartConfiguration.AzureOpenAIEmbeddingOptions.EmbeddingDeploymentName, QuickStartConfiguration.AzureOpenAIEmbeddingOptions.Endpoint, QuickStartConfiguration.AzureOpenAIEmbeddingOptions.ApiKey)
                .WithAzureChatCompletionService(QuickStartConfiguration.AzureOpenAIOptions.GPT35ModelDeploymentName, QuickStartConfiguration.AzureOpenAIOptions.Endpoint, QuickStartConfiguration.AzureOpenAIOptions.ApiKey)
                .Build();
        }

        [TestMethod]
        public async Task TextCompletion_Test()
        {

            string skillRootFolder = Path.Join(AppContext.BaseDirectory, "Skills");

            var skills = kernel.ImportSemanticSkillFromDirectory(skillRootFolder, "Default");

            string input = "根据小明的身份证显示，小明是1990年出生的。";

            string question = "请问小明在2000年的时候年龄是多少?";

            SKContext context = kernel.CreateNewContext();

            context.Variables["input"] = input;
            context.Variables["question"] = question;

            ISKFunction func = kernel.Func("Default", "QnA");

            var funcContext = await func.InvokeAsync(context);

            Console.WriteLine(funcContext.Result);
        }

        [TestMethod]
        public async Task SK_NewSyntax_Test()
        {
            string skillRootFolder = Path.Join(AppContext.BaseDirectory, "Skills");

            var skills = kernel.ImportSemanticSkillFromDirectory(skillRootFolder, "Default");

            string input = "根据小明的身份证显示，小明是1990年出生的。";

            string question = "请问小明在2000年的时候年龄是多少?";

            var variables = new ContextVariables();

            variables["input"] = input;
            variables["question"] = question;

            SKContext context = await kernel.RunAsync(variables, skills["QnA"]);

            Console.WriteLine(context);
        }
    }
}