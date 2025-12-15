using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(PiecewiseLinearGrayTransformDialog), Lifecycle.Singleton)]
public partial class PiecewiseLinearGrayTransformDialog : IContentControl
{
    public PiecewiseLinearGrayTransformDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    [ObservableProperty] private int _r1 = 70;
    [ObservableProperty] private int _s1 = 30;
    [ObservableProperty] private int _r2 = 180;
    [ObservableProperty] private int _s2 = 220;

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);
}
