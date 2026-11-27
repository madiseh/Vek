namespace Vek.ArchitectureTests;

[TestFixture]
public class DependencyTests
{
    [Test]
    public void Vek_ShouldNotDependOn_Docs()
    {
        var result = Types.InAssembly(typeof(VekComponentBase).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Vek.Docs", "Vek.Docs.Landing", "Vek.Docs.Showcases", "Vek.Docs.Server", "Vek.Docs.Wasm")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
