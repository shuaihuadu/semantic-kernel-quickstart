﻿// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Represents an input variable for prompt functions.
/// </summary>
public sealed class InputVariable
{
    /// <summary>The name of the variable.</summary>
    private string _name = string.Empty;
    /// <summary>The description of the variable.</summary>
    private string _description = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputVariable"/> class.
    /// </summary>
    public InputVariable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputVariable"/> class from an existing instance.
    /// </summary>
    /// <param name="inputVariable"></param>
    public InputVariable(InputVariable inputVariable)
    {
        this.Name = inputVariable.Name;
        this.Description = inputVariable.Description;
        this.Default = inputVariable.Default;
        this.IsRequired = inputVariable.IsRequired;
        this.JsonSchema = inputVariable.JsonSchema;
    }

    /// <summary>
    /// Gets or sets the name of the variable.
    /// </summary>
    /// <remarks>
    /// As an example, when using "{{$style}}", the name is "style".
    /// </remarks>
    [JsonPropertyName("name")]
    public string Name
    {
        get => this._name;
        set
        {
            this._name = value;
        }
    }

    /// <summary>
    /// Gets or sets a description of the variable.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description
    {
        get => this._description;
        set => this._description = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets a default value for the variable.
    /// </summary>
    [JsonPropertyName("default")]
    public object? Default { get; set; }

    /// <summary>
    /// Gets or sets whether the variable is considered required (rather than optional).
    /// </summary>
    /// <remarks>
    /// The default is true.
    /// </remarks>
    [JsonPropertyName("is_required")]
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets JSON Schema describing this variable.
    /// </summary>
    /// <remarks>
    /// This string will be deserialized into an instance of <see cref="KernelJsonSchema"/>.
    /// </remarks>
    [JsonPropertyName("json_schema")]
    public string? JsonSchema { get; set; }
}
