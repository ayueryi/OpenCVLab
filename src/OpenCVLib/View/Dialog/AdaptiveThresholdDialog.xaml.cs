using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using System.Windows;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(AdaptiveThresholdDialog), Lifecycle.Singleton)]
public partial class AdaptiveThresholdDialog : IContentControl
{
    public AdaptiveThresholdDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    [ObservableProperty] private int _blockSize = 5;

    [ObservableProperty] private int _selectedThresholdTypes = 0;

    [ObservableProperty] private int _thresholdMaxValue = 255;

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);

    private void InfoIcon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is FrameworkElement icon)
        {
            // 根据图标名称打开对应的 Popup
            var popup = icon.Name switch
            {
                "BlockSizeInfoIcon" => FindName("BlockSizePopup") as System.Windows.Controls.Primitives.Popup,
                "ThresholdTypeInfoIcon" => FindName("ThresholdTypePopup") as System.Windows.Controls.Primitives.Popup,
                "MaxValueInfoIcon" => FindName("MaxValuePopup") as System.Windows.Controls.Primitives.Popup,
                _ => null
            };

            if (popup != null)
            {
                popup.IsOpen = true;
            }
        }
    }

    private void InfoIcon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // Popup 的 StaysOpen="False" 会自动处理关闭
    }
}
