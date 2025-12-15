using CommunityToolkit.Mvvm.ComponentModel;

namespace Yu.UI.Controls.ContentDialog;

/// <summary>
/// 简单弹窗
/// </summary>
[ObservableObject]
public partial class TipDialog : Yu.UI.IContentControl
{
    #region ViewModels

    [ObservableProperty] private string _tipTitle = "提示";

    [ObservableProperty] private string _tipContent = string.Empty;

    [ObservableProperty] private string _confirmText = "确认";

    [ObservableProperty] private string _cancelText = "取消";

    #endregion

    #region Properties

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    #endregion

    public TipDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    private void Confirm_Click(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null!);

    private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null!);
}