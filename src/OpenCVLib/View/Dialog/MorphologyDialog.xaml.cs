using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(MorphologyDialog), Lifecycle.Singleton)]
public partial class MorphologyDialog : IContentControl
{
    public MorphologyDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    [ObservableProperty] private int _blockSize = 5;

    [ObservableProperty] private int _iterations = 1;

    [ObservableProperty] private int _selectedKernelShape = 0;

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);
}
