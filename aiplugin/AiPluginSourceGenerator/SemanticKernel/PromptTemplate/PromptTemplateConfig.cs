// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Provides the configuration information necessary to create a prompt template.
/// </summary>
/// <remarks>
/// A prompt template is a template that can be used to generate a prompt to be submitted to an AI service.
/// For basic prompts, the template may be supplied as a simple string. For more complex prompts, more information
/// is desirable for describing the prompt template, such as details on input variables expected by the template.
/// This can all be provided by a <see cref="PromptTemplateConfig"/>, where its <see cref="PromptTemplateConfig.Template"/>
/// is the prompt template string itself, then with other properties set with additional information. To create the
/// actual prompt template, a <see cref="IPromptTemplateFactory"/> is used to create an <see cref="IPromptTemplate"/>;
/// this is done automatically by the APIs that accept a <see cref="PromptTemplateConfig"/>, using a default template
/// factory that understands the <see cref="PromptTemplateConfig.SemanticKernelTemplateFormat"/> format, but with the
/// ability to supply other factories for interpreting other formats.
/// </remarks>
public sealed class PromptTemplateConfig
{
    /// <summary>The format of the prompt template.</summary>
    private string? _templateFormat;
    /// <summary>The prompt template string.</summary>
    private string _template = string.Empty;
    /// <summary>Lazily-initialized input variables.</summary>
    private List<InputVariable>? _inputVariables;
    /// <summary>Lazily-initialized execution settings. The key is the service ID, or <see cref="PromptExecutionSettings.DefaultServiceId"/> for the default execution settings.</summary>
    private Dictionary<string, PromptExecutionSettings>? _executionSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptTemplateConfig"/> class.
    /// </summary>
    public PromptTemplateConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptTemplateConfig"/> class using the specified prompt template string.
    /// </summary>
    /// <param name="template">The prompt template string that defines the prompt.</param>
    /// <exception cref="ArgumentNullException"><paramref name="template"/> is null.</exception>
    public PromptTemplateConfig(string template)
    {
        this.Template = template;
    }

    /// <summary>
    /// Gets or sets the function name to use by default when creating prompt functions using this configuration.
    /// </summary>
    /// <remarks>
    /// If the name is null or empty, a random name will be generated dynamically when creating a function.
    /// </remarks>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a function description to use by default when creating prompt functions using this configuration.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets the identifier of the Semantic Kernel template format.
    /// </summary>
    public static string SemanticKernelTemplateFormat => "semantic-kernel";

    /// <summary>
    /// Gets or sets the format of the prompt template.
    /// </summary>
    /// <remarks>
    /// If no template format is specified, a default format of <see cref="SemanticKernelTemplateFormat"/> is used.
    /// </remarks>
    [JsonPropertyName("template_format")]
    public string TemplateFormat
    {
        get => this._templateFormat ?? SemanticKernelTemplateFormat;
        set => this._templateFormat = value;
    }

    /// <summary>
    /// Gets or sets the prompt template string that defines the prompt.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    [JsonPropertyName("template")]
    public string Template
    {
        get => this._template;
        set
        {
            this._template = value;
        }
    }

    /// <summary>
    /// Gets or sets the collection of input variables used by the prompt template.
    /// </summary>
    [JsonPropertyName("input_variables")]
    public List<InputVariable> InputVariables
    {
        get => this._inputVariables ??= new();
        set
        {
            this._inputVariables = value;
        }
    }

    /// <summary>
    /// Gets or sets the output variable used by the prompt template.
    /// </summary>
    [JsonPropertyName("output_variable")]
    public OutputVariable? OutputVariable { get; set; }

    /// <summary>
    /// Gets or sets the collection of execution settings used by the prompt template.
    /// </summary>
    /// <remarks>
    /// The settings dictionary is keyed by the service ID, or <see cref="PromptExecutionSettings.DefaultServiceId"/> for the default execution settings.
    /// </remarks>
    [JsonPropertyName("execution_settings")]
    public Dictionary<string, PromptExecutionSettings> ExecutionSettings
    {
        get => this._executionSettings ??= new();
        set
        {
            this._executionSettings = value;
        }
    }

    /// <summary>
    /// Gets the default execution settings from <see cref="ExecutionSettings"/>.
    /// </summary>
    /// <remarks>
    /// If no default is specified, this will return null.
    /// </remarks>
    public PromptExecutionSettings? DefaultExecutionSettings => this._executionSettings?.TryGetValue(PromptExecutionSettings.DefaultServiceId, out PromptExecutionSettings? settings) is true ? settings : null;
}
