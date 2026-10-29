using System.Text.RegularExpressions;

namespace Vek.Components.Components.CodeView;

internal static partial class VekCodeHighlighter
{
    private static readonly string[] CSharpKeywords =
    [
        "abstract", "as", "async", "await", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
        "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
        "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
        "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly",
        "record", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
        "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
        "ushort", "using", "var", "virtual", "void", "volatile", "while", "yield"
    ];

    private static readonly string[] JavaKeywords =
    [
        "abstract", "assert", "boolean", "break", "byte", "case", "catch", "char", "class", "const",
        "continue", "default", "do", "double", "else", "enum", "extends", "final", "finally", "float", "for",
        "if", "implements", "import", "instanceof", "int", "interface", "long", "native", "new", "package",
        "private", "protected", "public", "return", "short", "static", "strictfp", "super", "switch",
        "synchronized", "this", "throw", "throws", "transient", "try", "void", "volatile", "while"
    ];

    private static readonly string[] SqlKeywords =
    [
        "add", "all", "alter", "and", "as", "asc", "begin", "between", "by", "case", "check", "commit",
        "constraint", "create", "cross", "current_date", "current_timestamp", "database", "default", "delete",
        "desc", "distinct", "drop", "else", "end", "exists", "foreign", "from", "full", "group", "having",
        "inner", "insert", "into", "is", "join", "key", "left", "like", "limit", "not", "null", "on", "or",
        "order", "outer", "primary", "references", "right", "rollback", "select", "set", "table", "then",
        "union", "unique", "update", "values", "when", "where"
    ];

    private static readonly Regex HtmlTokens = new(
        "(?<comment><!--[\\s\\S]*?-->)|(?<doctype><!DOCTYPE\\b[^>]*>)|(?<tag></?[A-Za-z][A-Za-z0-9:-]*(?:\\s+[^<>]*?)?\\/?>)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex CSharpTokens = new(
        BuildCodePattern(CSharpKeywords),
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex JavaTokens = new(
        BuildCodePattern(JavaKeywords),
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex SqlTokens = new(
        $"(?<comment>--[^\\r\\n]*|/\\*[\\s\\S]*?\\*/)|(?<string>'(?:''|[^'])*'|\"(?:\"\"|[^\"])*\")|(?<keyword>\\b(?:{BuildAlternation(SqlKeywords)})\\b)|(?<number>\\b\\d+(?:\\.\\d+)?\\b)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static IReadOnlyList<VekCodeToken> Highlight(string code, VekCodeLanguage language)
    {
        if (string.IsNullOrEmpty(code))
        {
            return [];
        }

        var (pattern, tagKind) = language switch
        {
            VekCodeLanguage.Html => (HtmlTokens, VekCodeTokenKind.Tag),
            VekCodeLanguage.Java => (JavaTokens, VekCodeTokenKind.Plain),
            VekCodeLanguage.CSharp => (CSharpTokens, VekCodeTokenKind.Plain),
            VekCodeLanguage.Sql => (SqlTokens, VekCodeTokenKind.Plain),
            _ => (null, VekCodeTokenKind.Plain)
        };

        if (pattern is null)
        {
            return [new VekCodeToken(code, VekCodeTokenKind.Plain)];
        }

        var tokens = new List<VekCodeToken>();
        var lastIndex = 0;

        foreach (Match match in pattern.Matches(code))
        {
            if (match.Index > lastIndex)
            {
                tokens.Add(new VekCodeToken(code[lastIndex..match.Index], VekCodeTokenKind.Plain));
            }

            var kind = match.Groups["comment"].Success ? VekCodeTokenKind.Comment
                : match.Groups["string"].Success ? VekCodeTokenKind.String
                : match.Groups["keyword"].Success ? VekCodeTokenKind.Keyword
                : match.Groups["number"].Success ? VekCodeTokenKind.Number
                : match.Groups["tag"].Success || match.Groups["doctype"].Success ? tagKind
                : VekCodeTokenKind.Plain;

            tokens.Add(new VekCodeToken(match.Value, kind));
            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < code.Length)
        {
            tokens.Add(new VekCodeToken(code[lastIndex..], VekCodeTokenKind.Plain));
        }

        return tokens;
    }

    private static string BuildAlternation(IEnumerable<string> keywords) =>
        string.Join('|', keywords.OrderByDescending(keyword => keyword.Length).Select(Regex.Escape));

    private static string BuildCodePattern(IEnumerable<string> keywords) =>
        $"(?<comment>//[^\\r\\n]*|/\\*[\\s\\S]*?\\*/)|(?<string>\"\"\"[\\s\\S]*?\"\"\"|@?\"(?:\"\"|\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*')|(?<keyword>\\b(?:{BuildAlternation(keywords)})\\b)|(?<number>\\b(?:0[xX][0-9A-Fa-f]+|\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?[fFdDmMlLuU]*)\\b)";
}
