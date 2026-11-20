namespace Vek.Docs.Compiler;

public static class Paths
{
    private const string DocsDirectory = "Vek.Docs";
    private const string SnippetsFile = "Snippets.generated.cs";
    private const string ApiDocumentationFile = "ApiDocumentation.generated.cs";
    private const string NewFilesToBuild = "NewFilesToBuild.txt";

    public const string ExampleDiscriminator = "Example"; // example components must contain this string

    private static string RepositoryRootPath
    {
        get
        {
            var workingPath = Path.GetFullPath(".");
            do
            {
                if (File.Exists(Path.Join(workingPath, "Vek.slnx")))
                {
                    return workingPath;
                }

                workingPath = Path.GetDirectoryName(workingPath);
            }
            while (!string.IsNullOrWhiteSpace(workingPath));

            throw new DirectoryNotFoundException("Could not locate Vek.slnx from the current working directory.");
        }
    }

    public static string SrcDirPath => Path.Join(RepositoryRootPath, "src");

    public static string DocsDirPath => Path.Join(SrcDirPath, "Docs", DocsDirectory);

    public static string DocsStringSnippetsDirPath => Path.Join(DocsDirPath, "Models");

    public static string SnippetsFilePath => Path.Join(DocsStringSnippetsDirPath, SnippetsFile);

    public static string NewFilesToBuildPath => Path.Join(DocsDirPath, NewFilesToBuild);

    public static string ApiDocumentationPath => Path.Join(DocsDirPath, "Models", "Generated");

    public static string ApiDocumentationFilePath => Path.Join(ApiDocumentationPath, ApiDocumentationFile);

    // Stamp files live in obj/ (git-ignored, never a Compile input). They record that the generator validated its output against the current inputs, so the generated .cs keeps its old timestamp when content is unchanged and does not force a full Docs recompile after every library rebuild.
    private static string StampDirPath => Path.Join(DocsDirPath, "obj");

    public static string SnippetsStampFilePath => Path.Join(StampDirPath, SnippetsFile + ".stamp");

    public static string ApiDocumentationStampFilePath => Path.Join(StampDirPath, ApiDocumentationFile + ".stamp");

    // Records that the generator has validated its output against the current inputs.
    public static void TouchStamp(string stampFilePath) => WriteStamp(stampFilePath, DateTime.UtcNow.ToString("O"));

    // Writes arbitrary content (e.g. an input hash) to a stamp file, creating the directory if needed.
    public static void WriteStamp(string stampFilePath, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(stampFilePath)!);
        File.WriteAllText(stampFilePath, content);
    }
}
