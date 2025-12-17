using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using OpenCVLab.Help;

using Yu.UI;

#pragma warning disable CS8625

namespace OpenCVLab.View.Dialog;

[ObservableObject]
[KeyedInject(typeof(IContentControl), nameof(PiecewiseLinearGrayTransformDialog), Lifecycle.Singleton)]
public partial class PiecewiseLinearGrayTransformDialog : IContentControl
{
    private HistogramVisualization? _histogramViz;

    public PiecewiseLinearGrayTransformDialog()
    {
        DataContext = this;
        InitializeSegments();
        InitializeComponent();

        // 初始化直方图可视化
        HistogramCanvas.Loaded += (s, e) =>
        {
            _histogramViz = new HistogramVisualization(HistogramCanvas);
            UpdateHistogram();
        };

        HistogramCanvas.SizeChanged += (s, e) => UpdateHistogram();
    }

    public Action<object>? CloseCallback { get; set; }
    public Action<object>? SuccCallback { get; set; }
    public Action<object>? FailCallback { get; set; }
    public Action<object>? CancelCallback { get; set; }

    /// <summary>
    /// 分段集合
    /// </summary>
    [ObservableProperty] private ObservableCollection<PiecewiseSegment> _segments = new();

    /// <summary>
    /// 当前选中的分段
    /// </summary>
    [ObservableProperty] private PiecewiseSegment? _selectedSegment;

    partial void OnSelectedSegmentChanged(PiecewiseSegment? value)
    {
        UpdateHistogram();
    }

    /// <summary>
    /// 初始化默认分段（两个分段）
    /// </summary>
    private void InitializeSegments()
    {
        Segments.Clear();
        // 第一段: [0, 70] -> [0, 30]
        var seg1 = new PiecewiseSegment(0, 70, 0, 30);
        // 第二段: [70, 180] -> [30, 220]
        var seg2 = new PiecewiseSegment(70, 180, 30, 220);
        // 第三段: [180, 255] -> [220, 255]
        var seg3 = new PiecewiseSegment(180, 255, 220, 255);

        seg1.PropertyChanged += (s, e) => UpdateHistogram();
        seg2.PropertyChanged += (s, e) => UpdateHistogram();
        seg3.PropertyChanged += (s, e) => UpdateHistogram();

        Segments.Add(seg1);
        Segments.Add(seg2);
        Segments.Add(seg3);

        SelectedSegment = Segments.FirstOrDefault();
    }

    /// <summary>
    /// 添加新分段
    /// </summary>
    [RelayCommand]
    private void AddSegment()
    {
        if (Segments.Count == 0)
        {
            var newSeg = new PiecewiseSegment(0, 255, 0, 255);
            newSeg.PropertyChanged += (s, e) => UpdateHistogram();
            Segments.Add(newSeg);
        }
        else
        {
            var lastSegment = Segments.Last();
            var newStart = Math.Min(lastSegment.InputEnd, 254);
            var newSeg = new PiecewiseSegment(newStart, 255, lastSegment.OutputEnd, 255);
            newSeg.PropertyChanged += (s, e) => UpdateHistogram();
            Segments.Add(newSeg);
        }
        SelectedSegment = Segments.Last();
        UpdateHistogram();
    }

    /// <summary>
    /// 删除选中的分段
    /// </summary>
    [RelayCommand]
    private void RemoveSegment()
    {
        if (SelectedSegment != null && Segments.Count > 1)
        {
            var index = Segments.IndexOf(SelectedSegment);
            Segments.Remove(SelectedSegment);
            SelectedSegment = index < Segments.Count ? Segments[index] : Segments.Last();
            UpdateHistogram();
        }
    }

    /// <summary>
    /// 重置为默认分段
    /// </summary>
    [RelayCommand]
    private void ResetSegments()
    {
        InitializeSegments();
        UpdateHistogram();
    }

    /// <summary>
    /// 更新直方图显示
    /// </summary>
    private void UpdateHistogram()
    {
        if (_histogramViz == null) return;

        // 更新选中分段的范围覆盖层
        if (SelectedSegment != null)
        {
            _histogramViz.SelectedInputMin = SelectedSegment.InputStart;
            _histogramViz.SelectedInputMax = SelectedSegment.InputEnd;
            _histogramViz.ShowRangeOverlay = true;
        }
        else
        {
            _histogramViz.ShowRangeOverlay = false;
        }

        _histogramViz.DrawPiecewiseTransform(Segments);
    }

    private void Confirm(object sender, System.Windows.RoutedEventArgs e) => SuccCallback?.Invoke(null);

    private void Cancel(object sender, System.Windows.RoutedEventArgs e) => CancelCallback?.Invoke(null);
}
