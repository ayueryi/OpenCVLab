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

    [ObservableProperty] private int _selectedThresholdType = 0;
    [ObservableProperty] private int _thresholdValue = 125;
    [ObservableProperty] private int _thresholdMaxValue = 255;
    [ObservableProperty] private int _blockSize = 11;
    [ObservableProperty] private int _adaptiveBinaryType = 0;

    /// <summary>
    /// 是否显示简单阈值参数（阈值输入框）
    /// Binary(0), BinaryInv(1), Trunc(2), Tozero(3), TozeroInv(4)
    /// </summary>
    public bool ShowSimpleThresholdParams => SelectedThresholdType >= 0 && SelectedThresholdType <= 4;

    /// <summary>
    /// 是否显示自适应阈值参数
    /// Adaptive(5)
    /// </summary>
    public bool ShowAdaptiveParams => SelectedThresholdType == 5;

    /// <summary>
    /// 获取阈值类型的描述信息
    /// </summary>
    public string ThresholdDescription => SelectedThresholdType switch
    {
        0 => "标准二值化：dst = maxval (if src > thresh), else 0",
        1 => "反转二值化：dst = 0 (if src > thresh), else maxval",
        2 => "截断二值化：dst = threshold (if src > thresh), else src",
        3 => "阈值至零：dst = src (if src > thresh), else 0",
        4 => "阈值至零反转：dst = 0 (if src > thresh), else src",
        5 => "自适应阈值：根据局部区域自动计算阈值，适用于光照不均匀的图像",
        6 => "Otsu阈值：使用Otsu算法自动选择最佳阈值，适用于双峰直方图",
        7 => "Triangle阈值：使用Triangle算法自动选择最佳阈值",
        _ => ""
    };

    partial void OnSelectedThresholdTypeChanged(int value)
    {
        OnPropertyChanged(nameof(ShowSimpleThresholdParams));
        OnPropertyChanged(nameof(ShowAdaptiveParams));
        OnPropertyChanged(nameof(ThresholdDescription));
    }

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);
}
