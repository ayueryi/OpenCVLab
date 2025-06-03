using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;
using OpenCVLab.Model;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[Inject]
[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(OperationChooseDialog), Lifecycle.Singleton)]
public partial class OperationChooseDialog : IContentControl
{
    public OperationChooseDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    #region 视图模型

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private ObservableCollection<Operation> _operationsCollection = [];

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private Operation? _selectOperation;

    #endregion

    #region 委托

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    #endregion

    #region 回调

    private void Cancel(object sender, System.Windows.RoutedEventArgs e)
    {
        CancelCallback?.Invoke(null);
    }

    #endregion

    private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        SuccCallback?.Invoke(null);
    }
}
