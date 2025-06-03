using CommunityToolkit.Mvvm.ComponentModel;

using MaterialDesignThemes.Wpf;

namespace Yu.UI.Controls.ContentDialog;

/// <summary>
/// 简单弹窗
/// </summary>
[ObservableObject]
public partial class TipDialog
{
    #region ViewModels

    [ObservableProperty] private string _tipTitle = "提示";

    [ObservableProperty] private string _tipContent = string.Empty;

    [ObservableProperty] private string _confirmText = "确认";

    [ObservableProperty] private string _cancelText = "取消";

    #endregion

    #region Properties

    public Action? SuccCallback;

    public Action? FailCallback;

    #endregion

    public TipDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public void DialogClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
    {
        if (eventArgs.Parameter == null)
        {
            return;
        }
        if ((string)eventArgs.Parameter == "Success")
        {
            SuccCallback?.Invoke();
        }
        if ((string)eventArgs.Parameter == "Cancel")
        {
            FailCallback?.Invoke();
        }
    }
}