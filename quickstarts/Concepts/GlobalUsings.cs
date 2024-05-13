﻿global using Azure;
global using Azure.AI.OpenAI;
global using Azure.Core.Pipeline;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.SemanticKernel;
global using Microsoft.SemanticKernel.Agents;
global using Microsoft.SemanticKernel.Agents.OpenAI;
global using Microsoft.SemanticKernel.AudioToText;
global using Microsoft.SemanticKernel.ChatCompletion;
global using Microsoft.SemanticKernel.Connectors.OpenAI;
global using Microsoft.SemanticKernel.Memory;
global using Microsoft.SemanticKernel.TextToAudio;
global using Resources;
global using Resources.Plugins;
global using SharedLibrary;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Globalization;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Security.Cryptography;
global using System.Text.Json;
global using System.Text.Json.Serialization.Metadata;
global using System.Text.RegularExpressions;
global using xRetry;
global using Xunit.Abstractions;
