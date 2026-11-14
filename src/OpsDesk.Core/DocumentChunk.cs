namespace OpsDesk.Core;

public sealed record DocumentChunk(
    string Id,
    string SourceUrl,
    string Title,
    string Heading,
    string Text);
