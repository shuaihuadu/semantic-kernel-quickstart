// Copyright (c) IdeaTech. All rights reserved.

namespace StepwisePlannerMigration.Options;


public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    public string DeploymentName { get; set; }
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
}
