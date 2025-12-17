using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(LinearGrayTransformDialog), Lifecycle.Singleton)]
public partial class LinearGrayTransformDialog : IContentControl
{
    private enum DragTarget
    {
        None,
        InMin,
        InMax
    }

    private DragTarget _dragTarget = DragTarget.None;
    private bool _isDragging;

    public LinearGrayTransformDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    [ObservableProperty] private int _outMin = 0;

    [ObservableProperty] private int _outMax = 255;

    [ObservableProperty] private int _inMin = 0;

    [ObservableProperty] private int _inMax = 255;

    [ObservableProperty] private BitmapSource? _histogramImageSource;

    [ObservableProperty] private int _selectedPresetIndex;

    public bool IsCustomMode => SelectedPresetIndex == 0;

    public double PresetAlpha => GetPresetAlphaBeta(SelectedPresetIndex).alpha;

    public double PresetBeta => GetPresetAlphaBeta(SelectedPresetIndex).beta;

    partial void OnSelectedPresetIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsCustomMode));
        OnPropertyChanged(nameof(PresetAlpha));
        OnPropertyChanged(nameof(PresetBeta));
    }

    private static (double alpha, double beta) GetPresetAlphaBeta(int selectedPresetIndex)
    {
        return selectedPresetIndex switch
        {
            1 => (1, 50),
            2 => (1, -50),
            3 => (1.5, 0),
            4 => (0.75, 0),
            5 => (-0.5, 0),
            6 => (-1, 255),
            _ => (1, 0)
        };
    }

    partial void OnInMinChanged(int value)
    {
        if (value > InMax)
            InMax = value;
    }

    partial void OnInMaxChanged(int value)
    {
        if (value < InMin)
            InMin = value;
    }

    private void HistogramMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (HistogramImage is null)
            return;

        var width = HistogramImage.ActualWidth;
        if (width <= 1e-6)
            return;

        var pos = e.GetPosition(HistogramImage);
        var gray = (int)Math.Round((pos.X / width) * 255);
        gray = Math.Clamp(gray, 0, 255);

        // Pick the closer marker as drag target.
        var distToMin = Math.Abs(gray - InMin);
        var distToMax = Math.Abs(gray - InMax);
        _dragTarget = distToMin <= distToMax ? DragTarget.InMin : DragTarget.InMax;

        // Apply immediately on mouse down (click-to-move) and start dragging.
        SetDragValue(gray);
        _isDragging = true;
        ((UIElement)sender).CaptureMouse();
        e.Handled = true;
    }

    private void HistogramMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _dragTarget == DragTarget.None)
            return;

        if (HistogramImage is null)
            return;

        var width = HistogramImage.ActualWidth;
        if (width <= 1e-6)
            return;

        var pos = e.GetPosition(HistogramImage);
        var gray = (int)Math.Round((pos.X / width) * 255);
        gray = Math.Clamp(gray, 0, 255);

        SetDragValue(gray);
        e.Handled = true;
    }

    private void HistogramMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        _dragTarget = DragTarget.None;
        ((UIElement)sender).ReleaseMouseCapture();
        e.Handled = true;
    }

    private void SetDragValue(int gray)
    {
        switch (_dragTarget)
        {
            case DragTarget.InMin:
                InMin = gray;
                break;
            case DragTarget.InMax:
                InMax = gray;
                break;
        }
    }

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);
}
