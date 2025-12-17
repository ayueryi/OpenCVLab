using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OpenCVLab.View.Dialog;

/// <summary>
/// 直方图可视化控件 - 用于显示分段线性变换的映射关系
/// 合并了范围覆盖层功能
/// </summary>
public class HistogramVisualization
{
    private readonly Canvas _canvas;
    private const double Padding = 20;

    /// <summary>
    /// 当前选中分段的输入范围（用于高亮显示）
    /// </summary>
    public int SelectedInputMin { get; set; } = 0;
    public int SelectedInputMax { get; set; } = 255;

    /// <summary>
    /// 是否显示范围覆盖层
    /// </summary>
    public bool ShowRangeOverlay { get; set; } = true;

    public HistogramVisualization(Canvas canvas)
    {
        _canvas = canvas;
    }

    /// <summary>
    /// 绘制分段线性变换的映射曲线
    /// </summary>
    public void DrawPiecewiseTransform(ObservableCollection<PiecewiseSegment> segments)
    {
        _canvas.Children.Clear();

        if (segments == null || segments.Count == 0)
            return;

        var width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 560;
        var height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 200;

        if (width <= 2 * Padding || height <= 2 * Padding)
            return;

        // 绘制坐标轴
        DrawAxes(width, height);

        // 绘制网格线
        DrawGrid(width, height);

        // 绘制分段线
        DrawSegments(segments, width, height);

        // 绘制范围覆盖层（如果启用）
        if (ShowRangeOverlay)
        {
            DrawRangeOverlay(width, height);
        }

        // 绘制标签
        DrawLabels(width, height);
    }

    /// <summary>
    /// 绘制坐标轴
    /// </summary>
    private void DrawAxes(double width, double height)
    {
        // X轴
        var xAxis = new Line
        {
            X1 = Padding,
            Y1 = height - Padding,
            X2 = width - Padding,
            Y2 = height - Padding,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
        _canvas.Children.Add(xAxis);

        // Y轴
        var yAxis = new Line
        {
            X1 = Padding,
            Y1 = Padding,
            X2 = Padding,
            Y2 = height - Padding,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
        _canvas.Children.Add(yAxis);
    }

    /// <summary>
    /// 绘制网格线
    /// </summary>
    private void DrawGrid(double width, double height)
    {
        var gridBrush = new SolidColorBrush(Color.FromArgb(50, 128, 128, 128));
        var chartWidth = width - 2 * Padding;
        var chartHeight = height - 2 * Padding;

        // 绘制5条横向网格线
        for (int i = 1; i < 5; i++)
        {
            var y = Padding + chartHeight * i / 5;
            var line = new Line
            {
                X1 = Padding,
                Y1 = y,
                X2 = width - Padding,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 2, 2 }
            };
            _canvas.Children.Add(line);
        }

        // 绘制5条纵向网格线
        for (int i = 1; i < 5; i++)
        {
            var x = Padding + chartWidth * i / 5;
            var line = new Line
            {
                X1 = x,
                Y1 = Padding,
                X2 = x,
                Y2 = height - Padding,
                Stroke = gridBrush,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 2, 2 }
            };
            _canvas.Children.Add(line);
        }
    }

    /// <summary>
    /// 绘制分段线
    /// </summary>
    private void DrawSegments(ObservableCollection<PiecewiseSegment> segments, double width, double height)
    {
        var chartWidth = width - 2 * Padding;
        var chartHeight = height - 2 * Padding;

        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
            StrokeThickness = 3,
            StrokeLineJoin = PenLineJoin.Round
        };

        // 按输入起始值排序分段
        var sortedSegments = segments.OrderBy(s => s.InputStart).ToList();

        foreach (var segment in sortedSegments)
        {
            // 起点
            var x1 = Padding + (segment.InputStart / 255.0) * chartWidth;
            var y1 = height - Padding - (segment.OutputStart / 255.0) * chartHeight;
            polyline.Points.Add(new Point(x1, y1));

            // 终点
            var x2 = Padding + (segment.InputEnd / 255.0) * chartWidth;
            var y2 = height - Padding - (segment.OutputEnd / 255.0) * chartHeight;
            polyline.Points.Add(new Point(x2, y2));

            // 绘制控制点
            DrawControlPoint(x1, y1, segment.InputStart, segment.OutputStart);
            DrawControlPoint(x2, y2, segment.InputEnd, segment.OutputEnd);
        }

        _canvas.Children.Add(polyline);
    }

    /// <summary>
    /// 绘制控制点
    /// </summary>
    private void DrawControlPoint(double x, double y, int inputValue, int outputValue)
    {
        var ellipse = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = new SolidColorBrush(Color.FromRgb(255, 87, 34)),
            Stroke = Brushes.White,
            StrokeThickness = 2
        };

        Canvas.SetLeft(ellipse, x - 4);
        Canvas.SetTop(ellipse, y - 4);
        _canvas.Children.Add(ellipse);

        // 添加工具提示
        ellipse.ToolTip = $"({inputValue}, {outputValue})";
    }

    /// <summary>
    /// 绘制范围覆盖层 - 显示当前选中分段的输入范围
    /// </summary>
    private void DrawRangeOverlay(double width, double height)
    {
        var chartWidth = width - 2 * Padding;
        var inMin = Math.Clamp(SelectedInputMin, 0, 255);
        var inMax = Math.Clamp(SelectedInputMax, 0, 255);

        if (inMin > inMax)
        {
            (inMin, inMax) = (inMax, inMin);
        }

        var xMin = Padding + (inMin / 255.0) * chartWidth;
        var xMax = Padding + (inMax / 255.0) * chartWidth;

        // 绘制最小值指示线（深色）
        var minLine = new Line
        {
            X1 = xMin,
            Y1 = Padding,
            X2 = xMin,
            Y2 = height - Padding,
            Stroke = new SolidColorBrush(Color.FromRgb(0, 191, 255)), // DeepSkyBlue
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 }
        };
        _canvas.Children.Add(minLine);

        // 绘制最大值指示线（半透明）
        var maxLine = new Line
        {
            X1 = xMax,
            Y1 = Padding,
            X2 = xMax,
            Y2 = height - Padding,
            Stroke = new SolidColorBrush(Color.FromArgb(140, 0, 191, 255)),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 2 }
        };
        _canvas.Children.Add(maxLine);

        // 绘制范围区域（半透明填充）
        if (xMax > xMin)
        {
            var rangeRect = new System.Windows.Shapes.Rectangle
            {
                Width = xMax - xMin,
                Height = height - 2 * Padding,
                Fill = new SolidColorBrush(Color.FromArgb(30, 0, 191, 255))
            };
            Canvas.SetLeft(rangeRect, xMin);
            Canvas.SetTop(rangeRect, Padding);
            _canvas.Children.Insert(0, rangeRect); // 插入到底层
        }
    }

    /// <summary>
    /// 绘制标签
    /// </summary>
    private void DrawLabels(double width, double height)
    {
        // X轴标签
        var xLabel = new TextBlock
        {
            Text = "输入灰度 (Input)",
            FontSize = 11,
            Foreground = Brushes.Gray
        };
        Canvas.SetLeft(xLabel, width / 2 - 40);
        Canvas.SetTop(xLabel, height - 5);
        _canvas.Children.Add(xLabel);

        // Y轴标签
        var yLabel = new TextBlock
        {
            Text = "输出",
            FontSize = 11,
            Foreground = Brushes.Gray
        };
        Canvas.SetLeft(yLabel, 5);
        Canvas.SetTop(yLabel, 5);
        _canvas.Children.Add(yLabel);

        // 刻度标签
        var labels = new[] { "0", "64", "128", "192", "255" };
        for (int i = 0; i < labels.Length; i++)
        {
            var label = new TextBlock
            {
                Text = labels[i],
                FontSize = 9,
                Foreground = Brushes.Gray
            };
            var pos = Padding + (width - 2 * Padding) * i / 4;
            Canvas.SetLeft(label, pos - 10);
            Canvas.SetTop(label, height - Padding + 5);
            _canvas.Children.Add(label);
        }
    }
}
