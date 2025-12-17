using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenCVLab.View.Controls;

/// <summary>
/// 直方图范围覆盖层控件 - 在直方图上显示选中的灰度范围
/// 用于 LinearGrayTransformDialog
/// </summary>
public sealed class HistogramRangeOverlayCanvas : Canvas
{
    public static readonly DependencyProperty InMinProperty = DependencyProperty.Register(
        nameof(InMin),
        typeof(int),
        typeof(HistogramRangeOverlayCanvas),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty InMaxProperty = DependencyProperty.Register(
        nameof(InMax),
        typeof(int),
        typeof(HistogramRangeOverlayCanvas),
        new FrameworkPropertyMetadata(255, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
        nameof(Stroke),
        typeof(Brush),
        typeof(HistogramRangeOverlayCanvas),
        new FrameworkPropertyMetadata(Brushes.DeepSkyBlue, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness),
        typeof(double),
        typeof(HistogramRangeOverlayCanvas),
        new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public int InMin
    {
        get => (int)GetValue(InMinProperty);
        set => SetValue(InMinProperty, value);
    }

    public int InMax
    {
        get => (int)GetValue(InMaxProperty);
        set => SetValue(InMaxProperty, value);
    }

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 1e-6 || height <= 1e-6)
            return;

        var inMin = Math.Clamp(InMin, 0, 255);
        var inMax = Math.Clamp(InMax, 0, 255);
        if (inMin > inMax)
        {
            (inMin, inMax) = (inMax, inMin);
        }

        var pixelWidth = Math.Max(1.0, width - 1.0);
        var xMin = (inMin / 255.0) * pixelWidth;
        var xMax = (inMax / 255.0) * pixelWidth;

        var insetMax = Math.Max(0.0, width - 1.0);
        xMin = Math.Clamp(xMin, 0.0, insetMax);
        xMax = Math.Clamp(xMax, 0.0, insetMax);

        var baseBrush = Stroke ?? Brushes.DeepSkyBlue;
        var minPen = new Pen(baseBrush, StrokeThickness)
        {
            StartLineCap = PenLineCap.Flat,
            EndLineCap = PenLineCap.Flat
        };

        var maxBrush = baseBrush.Clone();
        maxBrush.Opacity = 0.55;
        var maxPen = new Pen(maxBrush, StrokeThickness)
        {
            StartLineCap = PenLineCap.Flat,
            EndLineCap = PenLineCap.Flat
        };

        dc.DrawLine(minPen, new Point(xMin, 0), new Point(xMin, height));
        dc.DrawLine(maxPen, new Point(xMax, 0), new Point(xMax, height));
    }
}
