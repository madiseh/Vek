namespace Vek.UnitTests.Docs.Generator;

public static class Paths
{
    private const string DocsDirectory = "Vek.Docs";
    private const string TestDirectory = "Vek.UnitTests.Docs";
    private const string ComponentTestsFile = "ExampleDocsTests.generated.cs";
    private const string ApiPageTestsFile = "ApiDocsTests.generated.cs";

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

    public static string TestDirPath => Path.Join(RepositoryRootPath, "tests", "Docs", TestDirectory, "Generated");

    public static string ComponentTestsFilePath => Path.Join(TestDirPath, ComponentTestsFile);

    public static string ApiPageTestsFilePath => Path.Join(TestDirPath, ApiPageTestsFile);
}
