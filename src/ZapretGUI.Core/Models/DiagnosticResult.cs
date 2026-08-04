namespace ZapretGUI.Core.Models;

public enum DiagnosticSeverity
{
    Ok,
    Warning,
    Error
}

public sealed class DiagnosticResult
{
    public required string Category { get; init; }
    public required string Message { get; init; }
    public DiagnosticSeverity Severity { get; init; }
    public string? HelpUrl { get; init; }
}
