namespace Vek.Demo.Components.Pages.Docs.CodeView;

public partial class CodeView
{
    private const string HtmlSample = """
        <article class="card">
            <h2>Welcome</h2>
            <p>Browse the latest updates.</p>
        </article>
        """;

    private const string JavaSample = """
        public class Greeting {
            public static void main(String[] args) {
                System.out.println("Hello, Vek!");
            }
        }
        """;

    private const string CSharpSample = """
        public static class Greeting
        {
            public static string Create(string name) => $"Hello, {name}!";
        }
        """;

    private const string SqlSample = """
        SELECT name, created_at
        FROM users
        WHERE is_active = 1
        ORDER BY created_at DESC;
        """;
}
