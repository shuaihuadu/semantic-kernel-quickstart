namespace DocumentationExamples;

public class Prompts(ITestOutputHelper output) : BaseTest(output)
{
    [Fact]
    public async Task RunAsync()
    {
        WriteLine("======== Prompts ========");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();


        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        string prompt = $"What is the intent of the request? {request}";

        WriteLine("0.0 Initial prompt");
        WriteLine(await kernel.InvokePromptAsync(prompt));

        prompt = @$"What is the intent of the request?{request}
                    You can choose between SendEmail, SendMessage, CompleteTask, CreateDocument.";
        WriteLine("1.0 Make the prompt more specific");
        WriteLine(await kernel.InvokePromptAsync(prompt));

        prompt = $@"Instructions: What is the intent of the request?
        Choices: SendEmail, SendMessage, CompleteTask, CreateDocument.
        User Input:{request}
        Intent: ";
        WriteLine("2.0 Add structure to the output with formatting");
        WriteLine(await kernel.InvokePromptAsync(prompt));

        prompt = $@"## Instructions
Provide the intent of the request using the following format:
```json
{{
    ""intent"":{{intent}}
}}
```

## Choices
You can choose between the following intents:

```json
[""SendEmail"",""SendMessage"", ""CompleteTask"", ""CreateDocument""]
```

## User Input
The user input is:
```json
{{
    ""request"":""{request}""
}}
```

## Intent";
        WriteLine("2.1 Add structure to the output with formatting (using Markdown and JSON)");
        WriteLine(await kernel.InvokePromptAsync(prompt));

        prompt = @$"Instructions: What is the intent of this request?
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument.

User Input: Can you send a very quick approval to the marketing team?
Intent: SendMessage

User Input: Can you send the full update to the marketing team?
Intent: SendEmail

User Input: {request}
Intent: ";
        WriteLine("3.0 Provide examples with few-shot prompting");
        WriteLine(await kernel.InvokePromptAsync(prompt));


        prompt = @$"Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.

User Input: Can you send a very quick approval to the marketing team?
Intent: SendMessage

User Input: Can you send the full update to the marketing team?
Intent: SendEmail

User Input: {request}
Intent: ";
        WriteLine("4.0 Tell the AI what to do to avoid doing something wrong");
        WriteLine(await kernel.InvokePromptAsync(prompt));

        string history = @"User input: I hate sending emails, no one ever reads them.
AI response: I'm sorry to hear that. Messages may be a better way to communicate.";

        prompt = @$"Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.

User Input: Can you send a very quick approval to the marketing team?
Intent: SendMessage

User Input: Can you send the full update to the marketing team?
Intent: SendEmail

{history}
User Input: {request}
Intent: ";
        WriteLine("5.0 Provide context to the AI");
        WriteLine(await kernel.InvokePromptAsync(prompt));

        history = @"<message role=""user"">I hate sending emails, no one ever reads them.</message>
<message role=""assistant"">I'm sorry to hear that. Messages may be a better way to communicate.</message>";

        prompt = @$"<message role=""system"">Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.</message>

<message role=""user"">Can you send a very quick approval to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendMessage</message>

<message role=""user"">Can you send the full update to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendEmail</message>

{history}
<message role=""user"">{request}</message>
<message role=""system"">Intent:</message>";
        WriteLine("6.0 Using message roles in chat completion prompts");
        WriteLine(await kernel.InvokePromptAsync(prompt));


        history = @"<message role=""user"">I hate sending emails, no one ever reads them.</message>
<message role=""assistant"">I'm sorry to hear that. Messages may be a better way to communicate.</message>";

        prompt = @$"<message role=""system"">Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.
Bonus: You'll get $20 if you get this right.</message>

<message role=""user"">Can you send a very quick approval to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendMessage</message>

<message role=""user"">Can you send the full update to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendEmail</message>

{history}
<message role=""user"">{request}</message>
<message role=""system"">Intent:</message>";
        WriteLine("7.0 Give your AI words of encouragement");
        WriteLine(await kernel.InvokePromptAsync(prompt));
    }
}