using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[Inject]
[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(ColorConversionDialog), Lifecycle.Singleton)]
public partial class ColorConversionDialog : IContentControl
{
    public ColorConversionDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    #region 委托

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    #endregion

    #region 视图模型

    [ObservableProperty] private string _colorConversionName = "BGR2GRAY";

    [ObservableProperty]
    private ObservableCollection<string> _colorConversionCollection =
    [
        "BGR2GRAY",
        "BGR2RGB",
        "BGR2HSV",
    ];

    #endregion

    private void Confirm(object sender, System.Windows.RoutedEventArgs e)
    {
        SuccCallback?.Invoke(null);
    }

    private void Cancel(object sender, System.Windows.RoutedEventArgs e)
    {
        CancelCallback?.Invoke(null);
    }
}
