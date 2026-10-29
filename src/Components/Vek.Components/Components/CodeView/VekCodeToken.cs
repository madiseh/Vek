namespace Vek.Components.Components.CodeView;

internal enum VekCodeTokenKind
{
    Plain,
    Keyword,
    String,
    Comment,
    Number,
    Tag
}

internal readonly record struct VekCodeToken(string Text, VekCodeTokenKind Kind);
