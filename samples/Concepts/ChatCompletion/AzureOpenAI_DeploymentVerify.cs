using System.Net.Http.Headers;

namespace Concepts;

[TestClass]
public class AzureOpenAI_DeploymentVerify : BaseTest
{
    [TestMethod]
    public async Task TestAsync()
    {
        //Kernel kernel = Kernel.CreateBuilder()
        //    .AddAzureAIInferenceChatCompletion("DeepSeek-R1", "iIlfUvbNQ2sqEpHHN6uH3PwXVkTVwI0W", new Uri("https://DeepSeek-R1-yavvn.eastus2.models.ai.azure.com/chat/completions"))
        //    .Build();

        //FunctionResult result = await kernel.InvokePromptAsync("你好呀");

        //Console.WriteLine(result.ToString());

        await InvokeRequestResponseService("https://DeepSeek-R1-curzv.eastus2.models.ai.azure.com", "nYEpcGZMUVmmrVtXRj94mPchekiZqRQl");
        //await InvokeRequestResponseService("https://DeepSeek-R1-yavvn.eastus2.models.ai.azure.com", "iIlfUvbNQ2sqEpHHN6uH3PwXVkTVwI0W");
    }

    static async Task InvokeRequestResponseService(string endpoint, string apiKey)
    {
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
        };

        using (var client = new HttpClient(handler))
        {
            var requestBody = @"{
                  ""messages"": [
                    {
                      ""role"": ""user"",
                      ""content"": ""你是谁？""
                    }
                  ],
                  ""max_tokens"": 2048
                }";

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("A key should be provided to invoke the endpoint");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.BaseAddress = new Uri(endpoint);
            client.Timeout = TimeSpan.FromSeconds(600);

            var content = new StringContent(requestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync("/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Result: {0}", result);
            }
            else
            {
                Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                // Print the headers - they include the requert ID and the timestamp,
                // which are useful for debugging the failure
                Console.WriteLine(response.Headers.ToString());

                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
        }
    }
}