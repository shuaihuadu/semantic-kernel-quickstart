// Copyright (c) IdeaTech. All rights reserved.

using Microsoft.SemanticKernel.ChatCompletion;

namespace StepwisePlannerMigration.Services;

public interface IPlanProvider
{
    ChatHistory GetPlan(string fileName);
}
