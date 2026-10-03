#nullable enable

namespace MudShadcn.Showcase.Docs;

/// <summary>One tab of a multi-file example: a title and the example's component name.</summary>
public sealed record CodeFile(string Title, string Code);
