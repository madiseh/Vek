namespace Vek.Demo.Components.Pages.Docs.Blazor.CodeView;

public partial class MultiSample
{
    private readonly IReadOnlyList<VekCodeViewTab> _samples =
    [
        new("HTML", VekCodeLanguage.Html, """
            <article class="card">
                <h2>Welcome</h2>
                <p>Browse the latest updates.</p>
            </article>
            """),
        new("C#", VekCodeLanguage.CSharp, """
            public static class Greeting
            {
                public static string Create(string name) => $"Hello, {name}!";
            }
            """),
        new("SQL", VekCodeLanguage.Sql, """
            SELECT name, created_at
            FROM users
            WHERE is_active = 1
            ORDER BY created_at DESC;
            """)
    ];
}
