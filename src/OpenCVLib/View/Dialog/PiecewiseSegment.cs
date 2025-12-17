using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenCVLab.View.Dialog;

/// <summary>
/// 分段线性变换的单个分段
/// </summary>
public partial class PiecewiseSegment : ObservableObject
{
    /// <summary>
    /// 分段输入起始值 (0-255)
    /// </summary>
    [ObservableProperty] private int _inputStart;

    /// <summary>
    /// 分段输入结束值 (0-255)
    /// </summary>
    [ObservableProperty] private int _inputEnd;

    /// <summary>
    /// 分段输出起始值 (0-255)
    /// </summary>
    [ObservableProperty] private int _outputStart;

    /// <summary>
    /// 分段输出结束值 (0-255)
    /// </summary>
    [ObservableProperty] private int _outputEnd;

    /// <summary>
    /// 分段显示名称
    /// </summary>
    public string DisplayName => $"分段 [{InputStart}, {InputEnd}] → [{OutputStart}, {OutputEnd}]";

    public PiecewiseSegment(int inputStart, int inputEnd, int outputStart, int outputEnd)
    {
        InputStart = inputStart;
        InputEnd = inputEnd;
        OutputStart = outputStart;
        OutputEnd = outputEnd;
    }

    partial void OnInputStartChanged(int value) => OnPropertyChanged(nameof(DisplayName));
    partial void OnInputEndChanged(int value) => OnPropertyChanged(nameof(DisplayName));
    partial void OnOutputStartChanged(int value) => OnPropertyChanged(nameof(DisplayName));
    partial void OnOutputEndChanged(int value) => OnPropertyChanged(nameof(DisplayName));
}
