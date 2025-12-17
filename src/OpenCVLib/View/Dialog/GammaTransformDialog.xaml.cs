using System;
using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(GammaTransformDialog), Lifecycle.Singleton)]
public partial class GammaTransformDialog : IContentControl
{
    public GammaTransformDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    [ObservableProperty] private double _gamma = 1.0;

    [ObservableProperty] private System.Windows.Media.Imaging.BitmapSource? _histogramImageSource;

    [ObservableProperty] private int _selectedPresetIndex = 0;

    public bool IsCustomMode => SelectedPresetIndex == 0;

    public double PresetGamma => GetPresetGamma(SelectedPresetIndex);

    partial void OnSelectedPresetIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsCustomMode));
        OnPropertyChanged(nameof(PresetGamma));

        // 当选择预设时，自动更新Gamma值
        if (value != 0)
        {
            Gamma = PresetGamma;
        }
    }

    private static double GetPresetGamma(int selectedPresetIndex)
    {
        return selectedPresetIndex switch
        {
            1 => 0.4,     // 增强暗部
            2 => 0.67,    // 提亮图像
            3 => 1.0,     // 线性（不变）
            4 => 1.5,     // 压暗图像
            5 => 2.5,     // 增强亮部
            _ => 1.0      // 自定义默认值
        };
    }

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);
}
