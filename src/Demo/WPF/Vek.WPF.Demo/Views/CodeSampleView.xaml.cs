using System.Windows;

namespace Vek.WPF.Demo.Views;

public partial class CodeSampleView : UserControl
{
    public static readonly DependencyProperty SamplesProperty = DependencyProperty.Register(
        nameof(Samples), typeof(IReadOnlyList<CodeSample>), typeof(CodeSampleView), new PropertyMetadata(Array.Empty<CodeSample>()));

    public CodeSampleView()
    {
        InitializeComponent();
    }

    public IReadOnlyList<CodeSample> Samples
    {
        get => (IReadOnlyList<CodeSample>)GetValue(SamplesProperty);
        set => SetValue(SamplesProperty, value);
    }
}
