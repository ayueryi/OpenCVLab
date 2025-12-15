using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(AlgorithmParameterDialog), Lifecycle.Singleton)]
public partial class AlgorithmParameterDialog : IContentControl
{
    public AlgorithmParameterDialog()
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

    #region 参数

    [ObservableProperty]
    private int _selectedRetrievalModes = 0;

    [ObservableProperty]
    private int _selectedContourApproximationModes = 1;

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
