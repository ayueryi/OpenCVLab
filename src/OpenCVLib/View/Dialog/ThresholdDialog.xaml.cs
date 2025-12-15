using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(ThresholdDialog), Lifecycle.Singleton)]
public partial class ThresholdDialog : IContentControl
{
    public ThresholdDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    [ObservableProperty] private int _thresholdValue = 125;

    [ObservableProperty] private int _thresholdMaxValue = 255;

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);
}
