namespace Vek.WPF.Demo;

public partial class App : Application
{
    public IDemoProductProvider ProductProvider { get; } = new JsonDemoProductProvider();
}
