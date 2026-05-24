namespace TaskManager.Infrastructure.Services.AiAssistant;

/// <summary>
/// Configuration options for the AI assistant, bound from the <c>"Ai"</c> configuration section.
/// </summary>
public class AiOptions
{
    /// <summary>
    /// The configuration section name these options are bound from.
    /// </summary>
    public const string SectionName = "Ai";

    /// <summary>
    /// Gets or sets the model identifier used for AI completions. Defaults to <c>claude-haiku-4-5</c>.
    /// </summary>
    public string Model { get; set; } = "claude-haiku-4-5";

    /// <summary>
    /// Gets or sets the maximum number of output tokens for AI completions. Defaults to <c>1024</c>.
    /// </summary>
    public int MaxTokens { get; set; } = 1024;
}
