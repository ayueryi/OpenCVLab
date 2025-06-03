using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yu.UI.Controls;

/// <summary>
/// ImageViewer 提供图像预览功能，允许缩放，画布自适应等功能。
/// </summary>
public partial class ImageViewer : UserControl
{
    /// <summary>
    /// 每次使用矩阵的缩放比例。用于控制图像缩放的级别。
    /// </summary>
    private double Zooms = 1;

    /// <summary>
    /// 记录下最后一次鼠标左键按下的位置。用于实现拖动功能。
    /// </summary>
    private Point MarkPoint;

    /// <summary>
    /// 标记是否正在移动Canvas。用于确定鼠标移动事件是否需要执行移动操作。
    /// </summary>
    private bool MoveCanvas;

    public ImageViewer()
    {
        InitializeComponent();
    }

    #region Canvas缩放拖动

    /// <summary>
    /// 处理鼠标滚轮事件以实现图像缩放
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Canvas_Map_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        // 获取鼠标在Canvas上的位置
        Point pt2 = e.GetPosition(CanvasMap);
        MatrixTransform scal = new MatrixTransform();

        // 计算缩放比例，缩放因子根据滚轮滚动的delta值计算
        Zooms = 1 + e.Delta * 0.001;
        double offX = pt2.X - pt2.X * Zooms; // 计算X方向的偏移
        double offY = pt2.Y - pt2.Y * Zooms; // 计算Y方向的偏移

        // 获取Canvas的实际宽度和高度，并应用当前矩阵变换
        Point p = new Point(CanvasMap.ActualWidth, CanvasMap.ActualHeight);
        p = p * Matrix.Matrix;

        // 创建缩放矩阵
        scal.Matrix = new Matrix(Zooms, 0, 0, Zooms, offX, offY);

        // 更新Canvas的变换矩阵
        Matrix.Matrix = scal.Matrix * Matrix.Matrix;
    }

    /// <summary>
    /// 处理鼠标左键按下事件以开始移动Canvas
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Canvas_Map_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // 记录鼠标按下时的位置
        MarkPoint = e.GetPosition(CanvasMap);
        MoveCanvas = true; // 设置标志表示开始移动Canvas
    }

    /// <summary>
    /// 处理鼠标左键释放事件以停止移动Canvas
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Canvas_Map_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) => MoveCanvas = false;

    /// <summary>
    /// 处理鼠标移动事件以移动Canvas
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Canvas_Map_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        // 如果没有设置移动标志，退出函数
        if (!MoveCanvas)
        {
            return;
        }

        // 获取当前鼠标位置
        Point ptNow = e.GetPosition(CanvasMap);
        MatrixTransform scal = new MatrixTransform();

        // 计算Canvas在X和Y方向上的偏移量
        double offX = ptNow.X - MarkPoint.X;
        double offY = ptNow.Y - MarkPoint.Y;

        // 设置缩放比例为1（不缩放）
        Zooms = 1;
        MatrixTransform scal2 = new MatrixTransform();

        // 获取Canvas的实际宽度和高度，并应用当前矩阵变换
        Point p = new Point(CanvasMap.ActualWidth, CanvasMap.ActualHeight);
        p = p * Matrix.Matrix;

        // 创建平移矩阵
        scal.Matrix = new Matrix(Zooms, 0, 0, Zooms, offX, offY);

        // 更新Canvas的变换矩阵
        Matrix.Matrix = scal.Matrix * Matrix.Matrix;
    }

    #endregion

    #region 依赖属性

    /// <summary>
    /// ImageSource 的 DependencyProperty，用于绑定图像源。
    /// </summary>
    public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource",
                                        typeof(ImageSource),
                                        typeof(ImagePreviewControl),
                                        new PropertyMetadata(default(ImageSource),
                                        OnImageSourceChanged));

    /// <summary>
    /// 获取或设置 ImageSource 属性，该属性绑定到要显示的图像。
    /// </summary>
    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    #region 依赖属性更新事件

    /// <summary>
    /// 当 ImageSource 属性发生更改时的回调方法，用于更新显示的图像。
    /// </summary>
    /// <param name="d">引发更改的依赖对象。</param>
    /// <param name="e">包含事件数据的参数。</param>
    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ImagePreviewControl? control = d as ImagePreviewControl;
        if (control?.DrawingImage != null)
        {
            control.DrawingImage.Source = e.NewValue as ImageSource;
            control.ResizeImageToFit();
        }
    }

    #endregion

    #endregion

    #region 辅助方法

    /// <summary>
    /// 重置画布
    /// </summary>
    public void ResizeImageToFit()
    {
        // 获取 ScrollViewer 的实际尺寸
        double scrollViewerWidth = ScrollViewerMap.ActualWidth;
        double scrollViewerHeight = ScrollViewerMap.ActualHeight;

        // 获取 Image 的原始尺寸
        double imageWidth = DrawingImage.ActualWidth;
        double imageHeight = DrawingImage.ActualHeight;

        // 计算缩放因子
        double scaleX = scrollViewerWidth / imageWidth;
        double scaleY = scrollViewerHeight / imageHeight;

        // 使用较小的缩放因子保持比例
        double scale = Math.Min(scaleX, scaleY);

        // 设置矩阵转换
        Matrix matrix = new Matrix();
        matrix.Scale(scale, scale);
        Matrix.Matrix = matrix;

        // 重新设置 Canvas 的宽高
        CanvasMap.Width = imageWidth * scale;
        CanvasMap.Height = imageHeight * scale;
    }

    #endregion
}
