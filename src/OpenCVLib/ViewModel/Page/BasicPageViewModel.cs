using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using OpenCVLab.Help;
using OpenCVLab.Model;
using OpenCVLab.Model.DedectResult;
using OpenCVLab.Services.Operators;
using OpenCVLab.View.Dialog;
using OpenCVLab.View.Page;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using HandyControl.Controls;

using Yu.UI;

namespace OpenCVLab.ViewModel.Page;

[Inject]
public partial class BasicPageViewModel : ObservableObject
{
    private readonly IBasicOperatorService basicOperatorService;

    public BasicPageViewModel(IContentDialogService contentDialogService, IBasicOperatorService basicOperatorService)
    {
        this.contentDialogService = contentDialogService;
        this.basicOperatorService = basicOperatorService;
    }

    #region 视图模型

    [ObservableProperty] private BitmapSource? _histImageSource;

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private ObservableCollection<Operation> _operationsCollection = [];

    /// <summary>
    /// 
    /// </summary>
    [ObservableProperty] private Operation? _selectOperation;

    #region 算法参数

    /// <summary>
    /// 滤波器的大小
    /// </summary>
    [ObservableProperty] private int _blockSize = 5;

    /// <summary>
    /// 迭代次数
    /// </summary>
    [ObservableProperty] private int _iterations = 1;

    /// <summary>
    /// 形态学操作的内核形状
    /// </summary>
    [ObservableProperty] private int _selectedKernelShape = 0;

    /// <summary>
    /// 阈值处理的方法类型
    /// </summary>
    [ObservableProperty] private int _selectedThresholdTypes = 0;

    /// <summary>
    /// 二值化阈值
    /// </summary>
    [ObservableProperty] private int _thresholdValue = 125;

    /// <summary>
    /// 二值化赋值
    /// </summary>
    [ObservableProperty] private int _ThresholdMaxValue = 255;

    /// <summary>
    /// 线性灰度变换：输出最小灰度
    /// </summary>
    [ObservableProperty] private int _linearOutMin = 0;

    /// <summary>
    /// 线性灰度变换：输出最大灰度
    /// </summary>
    [ObservableProperty] private int _linearOutMax = 255;

    /// <summary>
    /// 线性灰度变换：输入最小灰度（用于直方图选点/裁剪）
    /// </summary>
    [ObservableProperty] private int _linearInMin = 0;

    /// <summary>
    /// 线性灰度变换：输入最大灰度（用于直方图选点/裁剪）
    /// </summary>
    [ObservableProperty] private int _linearInMax = 255;

    /// <summary>
    /// 线性灰度变换：预设选择（0=自定义）
    /// </summary>
    [ObservableProperty] private int _linearPresetIndex = 0;

    [ObservableProperty] private double _linearPresetAlpha = 1;

    [ObservableProperty] private double _linearPresetBeta = 0;

    /// <summary>
    /// 分段线性灰度变换参数：r1
    /// </summary>
    [ObservableProperty] private int _piecewiseR1 = 70;

    /// <summary>
    /// 分段线性灰度变换参数：s1
    /// </summary>
    [ObservableProperty] private int _piecewiseS1 = 30;

    /// <summary>
    /// 分段线性灰度变换参数：r2
    /// </summary>
    [ObservableProperty] private int _piecewiseR2 = 180;

    /// <summary>
    /// 分段线性灰度变换参数：s2
    /// </summary>
    [ObservableProperty] private int _piecewiseS2 = 220;

    #endregion

    #region 算法参数

    [ObservableProperty]
    private int _selectedRetrievalModes = 0;

    [ObservableProperty]
    private int _selectedContourApproximationModes = 1;

    #endregion

    #region 视图模型回调

    partial void OnSelectOperationChanged(Operation? oldValue, Operation? newValue)
    {
        Mat? src = SelectOperation?.ImageMat;

        if (src == null || src.Empty())
        {
            // 处理图像为空的情况
            return;
        }

        try
        {
            using var histImage = basicOperatorService.BuildChannelHistogramImage(src, histSize: 256, width: 300, height: 150);
            HistImageSource = histImage.ToBitmapSource();
        }
        catch
        {
            // 不阻塞 UI；直方图失败时忽略
        }
    }

    #endregion

    #endregion

    #region 属性

    public IContentDialogService contentDialogService { get; set; }

    /// <summary>
    /// 内核形状
    /// </summary>
    public MorphShapes KernelShape
    {
        get => (MorphShapes)SelectedKernelShape;
        set => SelectedKernelShape = (int)value;
    }

    #endregion

    #region 命令

    #region 文件操作

    /// <summary>
    /// 打开图像
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task OpenImage()
    {
        // 打开文件对话框
        OpenFileDialog ofd = new OpenFileDialog()
        {
            Filter = "图像文件|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.gif;*.ico;*.wdp;*.jxr;*.hdp;*.webp"
        };

        if (ofd.ShowDialog() is false)
        {
            return Task.CompletedTask;
        }

        try
        {
            Mat tempMat = new Mat(ofd.FileName, ImreadModes.AnyColor);
            string fileName = Path.GetFileName(ofd.FileName);

            if (tempMat is null)
            {
                Growl.ErrorGlobal("图像读取失败");
                return Task.CompletedTask;
            }

            Operation operation = new Operation();
            operation.ImageMat = tempMat;
            operation.ImageName = fileName;

            OperationsCollection.Add(operation);
            SelectOperation = operation;

            Growl.InfoGlobal($"读取文件成功：{ofd.FileName}");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal(ofd.FileName + "图像读取失败" + exception.Message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 保存图像
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task SaveImage()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty())
        {
            Growl.ErrorGlobal("没有图像可以保存");
            return Task.CompletedTask;
        }

        // 保存文件对话框
        SaveFileDialog sfd = new SaveFileDialog()
        {
            Filter = "图像文件|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.gif;*.ico;*.wdp;*.jxr;*.hdp;*.webp"
        };

        if (sfd.ShowDialog() == true)
        {
            SelectOperation.ImageMat.SaveImage(sfd.FileName);
        }

        return Task.CompletedTask;
    }

    #endregion

    #region 基础操作

    #region 颜色空间转换

    [RelayCommand]
    private async Task ConvertColor()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(ColorConversionDialog)) is not ColorConversionDialog colorConversionDialog)
        {
            Growl.ErrorGlobal("颜色空间转换对话框未找到");
            return;
        }

        var confirmed = await ShowConfirmDialogAsync(colorConversionDialog);
        if (!confirmed)
        {
            return;
        }

        var requestedName = colorConversionDialog.ColorConversionName;
        Enum.TryParse(requestedName, out ColorConversionCodes colorConversionCode);

        // Most images load as BGR, but some formats (e.g. PNG) may load as BGRA.
        // If the user selects a BGR conversion against a 4-channel source, OpenCV will throw.
        // Map the common cases so the operation still produces an output.
        var srcChannels = SelectOperation.ImageMat.Channels();
        if (srcChannels == 4 && string.Equals(requestedName, "BGR2GRAY", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse("BGRA2GRAY", out ColorConversionCodes bgra2Gray))
                colorConversionCode = bgra2Gray;
            requestedName = "BGRA2GRAY";
        }
        try
        {
            var result = basicOperatorService.ConvertColor(SelectOperation.ImageMat, colorConversionCode, requestedName);
            AddDerivedOperation(result.Mat, result.Suffix);
            Growl.SuccessGlobal($"颜色空间转换完成: {requestedName}");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("颜色空间转换失败: " + exception.Message);
        }
    }

    #endregion

    #region 滤波

    /// <summary>
    /// 归一化滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task NormalizedBlockFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmFilterKernelAsync();
        if (!confirmed) return;

        if (BlockSize <= 0)
        {
            Growl.ErrorGlobal("卷积核大小必须大于 0");
            return;
        }

        try
        {
            var result = basicOperatorService.Blur(SelectOperation.ImageMat, BlockSize);
            if (result.Mat.Empty())
            {
                Growl.ErrorGlobal("滤波结果为空");
                return;
            }

            AddDerivedOperation(result.Mat, result.Suffix);
            Growl.SuccessGlobal($"归一化滤波完成，卷积核: {BlockSize}");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("归一化滤波失败: " + exception.Message);
        }
    }

    /// <summary>
    /// 高斯滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GaussianFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmFilterKernelAsync();
        if (!confirmed) return;

        if (BlockSize <= 0)
        {
            Growl.ErrorGlobal("卷积核大小必须大于 0");
            return;
        }

        if (BlockSize % 2 == 0)
        {
            Growl.ErrorGlobal("高斯滤波卷积核大小必须为奇数");
            return;
        }

        try
        {
            var result = basicOperatorService.GaussianBlur(SelectOperation.ImageMat, BlockSize);
            AddDerivedOperation(result.Mat, result.Suffix);
            Growl.SuccessGlobal($"高斯滤波完成，卷积核: {BlockSize}");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("高斯滤波失败: " + exception.Message);
        }
    }

    /// <summary>
    /// 中值滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task MedianFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmFilterKernelAsync();
        if (!confirmed) return;

        if (BlockSize <= 1)
        {
            Growl.ErrorGlobal("中值滤波核大小必须大于 1");
            return;
        }

        if (BlockSize % 2 == 0)
        {
            Growl.ErrorGlobal("中值滤波核大小必须为奇数");
            return;
        }
        try
        {
            var result = basicOperatorService.MedianBlur(SelectOperation.ImageMat, BlockSize);
            AddDerivedOperation(result.Mat, result.Suffix);
            Growl.SuccessGlobal($"中值滤波完成，卷积核: {BlockSize}");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("中值滤波失败: " + exception.Message);
        }
    }

    /// <summary>
    /// 双边滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task BilateralFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmFilterKernelAsync();
        if (!confirmed) return;

        if (BlockSize <= 0)
        {
            Growl.ErrorGlobal("双边滤波参数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.BilateralFilter(SelectOperation.ImageMat, BlockSize);
            AddDerivedOperation(result.Mat, result.Suffix);
            Growl.SuccessGlobal($"双边滤波完成，参数: {BlockSize}");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("双边滤波失败: " + exception.Message);
        }
    }

    #endregion

    #region 基础形态学

    /// <summary>
    /// 腐蚀
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task Erode()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmMorphologyAsync();
        if (!confirmed) return;

        if (BlockSize <= 0 || Iterations <= 0)
        {
            Growl.ErrorGlobal("卷积核大小/次数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.Erode(SelectOperation.ImageMat, KernelShape, BlockSize, Iterations, SelectedKernelShape);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("腐蚀失败" + exception.Message);
        }
    }

    /// <summary>
    /// 膨胀
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task Dilate()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmMorphologyAsync();
        if (!confirmed) return;

        if (BlockSize <= 0 || Iterations <= 0)
        {
            Growl.ErrorGlobal("卷积核大小/次数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.Dilate(SelectOperation.ImageMat, KernelShape, BlockSize, Iterations, SelectedKernelShape);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("膨胀失败" + exception.Message);
        }
    }

    #endregion

    #region 高阶形态学

    /// <summary>
    /// 开运算
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task MorphologyExOpen()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmMorphologyAsync();
        if (!confirmed) return;

        if (BlockSize <= 0 || Iterations <= 0)
        {
            Growl.ErrorGlobal("卷积核大小/次数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.MorphologyEx(SelectOperation.ImageMat, MorphTypes.Open, KernelShape, BlockSize, Iterations, SelectedKernelShape);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[开运算]操作失败" + exception.Message);
        }
    }

    /// <summary>
    /// 闭运算
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task MorphologyExClose()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmMorphologyAsync();
        if (!confirmed) return;

        if (BlockSize <= 0 || Iterations <= 0)
        {
            Growl.ErrorGlobal("卷积核大小/次数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.MorphologyEx(SelectOperation.ImageMat, MorphTypes.Close, KernelShape, BlockSize, Iterations, SelectedKernelShape);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[闭运算]操作失败" + exception.Message);
        }
    }

    /// <summary>
    /// 梯度
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task MorphologyExGradient()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmMorphologyAsync();
        if (!confirmed) return;

        if (BlockSize <= 0 || Iterations <= 0)
        {
            Growl.ErrorGlobal("卷积核大小/次数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.MorphologyEx(SelectOperation.ImageMat, MorphTypes.Gradient, KernelShape, BlockSize, Iterations, SelectedKernelShape);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[梯度]操作失败" + exception.Message);
        }
    }

    /// <summary>
    /// 顶帽
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task MorphologyExTopHat()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmMorphologyAsync();
        if (!confirmed) return;

        if (BlockSize <= 0 || Iterations <= 0)
        {
            Growl.ErrorGlobal("卷积核大小/次数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.MorphologyEx(SelectOperation.ImageMat, MorphTypes.TopHat, KernelShape, BlockSize, Iterations, SelectedKernelShape);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[顶帽]操作失败" + exception.Message);
        }
    }

    /// <summary>
    /// 黑帽
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task MorphologyExBlackHat()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmMorphologyAsync();
        if (!confirmed) return;

        if (BlockSize <= 0 || Iterations <= 0)
        {
            Growl.ErrorGlobal("卷积核大小/次数必须大于 0");
            return;
        }
        try
        {
            var result = basicOperatorService.MorphologyEx(SelectOperation.ImageMat, MorphTypes.BlackHat, KernelShape, BlockSize, Iterations, SelectedKernelShape);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[黑帽]操作失败" + exception.Message);
        }
    }
    #endregion

    #endregion

    #region 阈值处理

    /// <summary>
    /// 统一的二值化处理入口
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ThresholdProcessing()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(ThresholdDialog)) is not ThresholdDialog dialog)
        {
            Growl.ErrorGlobal("阈值处理对话框未找到");
            return;
        }

        // 初始化对话框参数
        dialog.ThresholdValue = ThresholdValue;
        dialog.ThresholdMaxValue = ThresholdMaxValue;
        dialog.BlockSize = BlockSize;

        var confirmed = await ShowConfirmDialogAsync(dialog);
        if (!confirmed) return;

        // 保存参数
        ThresholdValue = dialog.ThresholdValue;
        ThresholdMaxValue = dialog.ThresholdMaxValue;
        BlockSize = dialog.BlockSize;

        try
        {
            OperatorResult result;
            string operationName;

            switch (dialog.SelectedThresholdType)
            {
                case 0: // Binary
                    result = basicOperatorService.Threshold(SelectOperation.ImageMat, ThresholdTypes.Binary, ThresholdValue, ThresholdMaxValue, "Binary");
                    operationName = "标准二值化";
                    break;
                case 1: // BinaryInv
                    result = basicOperatorService.Threshold(SelectOperation.ImageMat, ThresholdTypes.BinaryInv, ThresholdValue, ThresholdMaxValue, "BinaryInv");
                    operationName = "反转二值化";
                    break;
                case 2: // Trunc
                    result = basicOperatorService.Threshold(SelectOperation.ImageMat, ThresholdTypes.Trunc, ThresholdValue, ThresholdMaxValue, "Trunc");
                    operationName = "截断二值化";
                    break;
                case 3: // Tozero
                    result = basicOperatorService.Threshold(SelectOperation.ImageMat, ThresholdTypes.Tozero, ThresholdValue, ThresholdMaxValue, "Tozero");
                    operationName = "阈值至零";
                    break;
                case 4: // TozeroInv
                    result = basicOperatorService.Threshold(SelectOperation.ImageMat, ThresholdTypes.TozeroInv, ThresholdValue, ThresholdMaxValue, "TozeroInv");
                    operationName = "阈值至零反转";
                    break;
                case 5: // Adaptive
                    if (BlockSize <= 1 || BlockSize % 2 == 0)
                    {
                        Growl.ErrorGlobal("自适应阈值 BlockSize 必须为大于 1 的奇数");
                        return;
                    }
                    var adaptiveBinaryType = dialog.AdaptiveBinaryType == 0 ? ThresholdTypes.Binary : ThresholdTypes.BinaryInv;
                    result = basicOperatorService.AdaptiveThreshold(
                        SelectOperation.ImageMat,
                        AdaptiveThresholdTypes.GaussianC,
                        adaptiveBinaryType,
                        BlockSize,
                        ThresholdMaxValue,
                        dialog.AdaptiveBinaryType);
                    operationName = "自适应阈值";
                    break;
                case 6: // Otsu
                    result = basicOperatorService.OtsuThreshold(SelectOperation.ImageMat, ThresholdMaxValue);
                    operationName = "Otsu阈值";
                    break;
                case 7: // Triangle
                    result = basicOperatorService.TriangleThreshold(SelectOperation.ImageMat, ThresholdMaxValue);
                    operationName = "Triangle阈值";
                    break;
                default:
                    Growl.ErrorGlobal("未知的阈值类型");
                    return;
            }

            AddDerivedOperation(result.Mat, result.Suffix);
            Growl.SuccessGlobal($"{operationName}处理完成");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("阈值处理失败: " + exception.Message);
        }
    }

    /// <summary>
    /// 反色变换（图像反转）
    /// </summary>
    [RelayCommand]
    private Task Invert()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty())
        {
            Growl.ErrorGlobal("图像为空");
            return Task.CompletedTask;
        }

        try
        {
            var result = basicOperatorService.Invert(SelectOperation.ImageMat);
            AddDerivedOperation(result.Mat, result.Suffix);
            Growl.SuccessGlobal("反色变换完成");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("反色变换失败: " + exception.Message);
        }

        return Task.CompletedTask;
    }
    #endregion

    #region 直方图

    /// <summary>
    /// 绘制直方图
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task DrawHist()
    {
        Mat? src = SelectOperation?.ImageMat;

        if (src is null || src.Empty() || src.Channels() > 1)
        {
            Growl.ErrorGlobal("无法读取图像！");
            return Task.CompletedTask;
        }

        try
        {
            // 定义直方图参数
            int histSize = 256;
            Rangef histRange = new Rangef(0, 256);
            bool uniform = true;
            bool accumulate = false;

            // 计算直方图
            Mat[] images = { src };
            Mat hist = new Mat();
            int[] channels = { 0 };
            Mat mask = new Mat();
            Cv2.CalcHist(images, channels, mask, hist, 1, new int[] { histSize }, new Rangef[] { histRange }, uniform, accumulate);

            // 创建直方图图像
            int hist_w = 512;
            int hist_h = 400;
            int bin_w = (int)Math.Round((double)hist_w / histSize);

            Mat histImage = new Mat(hist_h, hist_w, MatType.CV_8UC3, Scalar.All(0));

            // 归一化直方图
            Cv2.Normalize(hist, hist, 0, histImage.Rows, NormTypes.MinMax, -1, new Mat());

            // 绘制直方图
            for (int i = 1; i < histSize; i++)
            {
                Cv2.Line(histImage, new Point(bin_w * (i - 1), hist_h - (int)Math.Round(hist.Get<float>(i - 1))),
                                 new Point(bin_w * (i), hist_h - (int)Math.Round(hist.Get<float>(i))),
                                 new Scalar(255, 255, 255), 2, LineTypes.AntiAlias, 0);
            }

            HistImageSource = histImage.ToBitmapSource();
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("绘制直方图异常:" + exception.Message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 绘制2d直方图
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Draw2dHist()
    {
        Mat? src = SelectOperation?.ImageMat;

        if (src is null || src.Empty() || src.Channels() < 2)
        {
            Growl.ErrorGlobal("无法读取图像或图像通道数不足 2！");
            return Task.CompletedTask;
        }

        try
        {
            // 定义直方图参数
            int[] histSize = { 256, 256 }; // 两个维度的直方图大小
            Rangef[] histRange = { new Rangef(0, 256), new Rangef(0, 256) }; // 两个维度的范围
            bool uniform = true;
            bool accumulate = false;

            // 计算 2D 直方图
            Mat[] images = { src };
            Mat hist = new Mat();
            int[] channels = { 0, 1 }; // 选择两个通道
            Mat mask = new Mat();
            Cv2.CalcHist(images, channels, mask, hist, 2, histSize, histRange, uniform, accumulate);

            // 归一化直方图
            Cv2.Normalize(hist, hist, 0, 255, NormTypes.MinMax, -1, new Mat());

            // 创建直方图图像
            int hist_w = 512;
            int hist_h = 512;
            Mat histImage = new Mat(hist_h, hist_w, MatType.CV_8UC3, Scalar.All(0));

            // 绘制 2D 直方图
            for (int i = 0; i < histSize[0]; i++)
            {
                for (int j = 0; j < histSize[1]; j++)
                {
                    int intensity = (int)hist.Get<float>(i, j);
                    Scalar color = new Scalar(intensity, intensity, intensity);
                    histImage.Set(i * (hist_h / histSize[0]), j * (hist_w / histSize[1]), color);
                }
            }

            HistImageSource = histImage.ToBitmapSource();
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("绘制 2D 直方图异常:" + exception.Message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 直方图均衡化
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task EqualizeHist()
    {
        Mat? src = SelectOperation?.ImageMat;

        if (src is null || src.Empty() || src.Channels() > 1)
        {
            Growl.ErrorGlobal("无法读取图像！");
            return Task.CompletedTask;
        }

        try
        {
            var result = basicOperatorService.EqualizeHist(src);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal($"{exception.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 自适应直方图均衡
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task CreateCLAHE()
    {
        Mat? src = SelectOperation?.ImageMat;

        if (src is null || src.Empty() || src.Channels() > 1)
        {
            Growl.ErrorGlobal("无法读取图像！");
            return Task.CompletedTask;
        }

        try
        {
            var result = basicOperatorService.CreateClahe(src);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal($"{exception.Message}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 图像灰度的线性变换（线性拉伸/对比度拉伸）
    /// </summary>
    [RelayCommand]
    private async Task LinearGrayTransform()
    {
        Mat? src = SelectOperation?.ImageMat;

        if (src is null || src.Empty() || src.Channels() > 1)
        {
            Growl.ErrorGlobal("无法读取图像！");
            return;
        }

        var confirmed = await ConfirmLinearGrayTransformAsync();
        if (!confirmed) return;

        // Preset alpha/beta mode
        if (LinearPresetIndex != 0)
        {
            try
            {
                var result = basicOperatorService.LinearAlphaBetaTransform(src, LinearPresetAlpha, LinearPresetBeta);
                AddDerivedOperation(result.Mat, result.Suffix);
            }
            catch (Exception exception)
            {
                Growl.ErrorGlobal("线性预设变换失败：" + exception.Message);
            }

            return;
        }

        if (LinearInMin is < 0 or > 255 || LinearInMax is < 0 or > 255)
        {
            Growl.ErrorGlobal("输入灰度范围必须在 0~255 之间");
            return;
        }

        if (LinearInMin >= LinearInMax)
        {
            Growl.ErrorGlobal("输入最小灰度必须小于输入最大灰度");
            return;
        }

        if (LinearOutMin is < 0 or > 255 || LinearOutMax is < 0 or > 255)
        {
            Growl.ErrorGlobal("输出灰度范围必须在 0~255 之间");
            return;
        }

        if (LinearOutMin >= LinearOutMax)
        {
            Growl.ErrorGlobal("输出最小灰度必须小于输出最大灰度");
            return;
        }

        try
        {
            var result = basicOperatorService.LinearGrayTransform(src, LinearInMin, LinearInMax, LinearOutMin, LinearOutMax);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (InvalidOperationException)
        {
            Growl.ErrorGlobal("图像灰度范围为常量，无法进行线性拉伸");
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("灰度线性变换失败：" + exception.Message);
        }
    }

    /// <summary>
    /// 图像分段线性灰度变换
    /// (0,0) -> (r1,s1) -> (r2,s2) -> (255,255)
    /// </summary>
    [RelayCommand]
    private async Task PiecewiseLinearGrayTransform()
    {
        Mat? src = SelectOperation?.ImageMat;

        if (src is null || src.Empty() || src.Channels() > 1)
        {
            Growl.ErrorGlobal("无法读取图像！");
            return;
        }

        var confirmed = await ConfirmPiecewiseLinearGrayTransformAsync();
        if (!confirmed) return;

        if (PiecewiseR1 is < 0 or > 255 || PiecewiseR2 is < 0 or > 255 ||
            PiecewiseS1 is < 0 or > 255 || PiecewiseS2 is < 0 or > 255)
        {
            Growl.ErrorGlobal("参数必须在 0~255 之间");
            return;
        }

        if (PiecewiseR1 >= PiecewiseR2)
        {
            Growl.ErrorGlobal("必须满足 r1 < r2");
            return;
        }

        try
        {
            // 使用对话框返回的参数执行变换
            var result = basicOperatorService.PiecewiseLinearGrayTransform(src, PiecewiseR1, PiecewiseS1, PiecewiseR2, PiecewiseS2);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("分段线性灰度变换失败：" + exception.Message);
        }
    }

    #endregion

    #region 边缘检测

    /// <summary>
    /// Canny边缘检测
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task Canny()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return;

        var confirmed = await ConfirmCannyAsync();
        if (!confirmed) return;
        try
        {
            var result = basicOperatorService.Canny(SelectOperation.ImageMat, ThresholdValue, ThresholdMaxValue);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Canny边缘检测失败" + exception.Message);
        }
    }

    /// <summary>
    /// Sobel边缘检测
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Sobel()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            var result = basicOperatorService.Sobel(SelectOperation.ImageMat);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Sobel边缘检测失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Laplacian边缘检测
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Laplacian()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            var result = basicOperatorService.Laplacian(SelectOperation.ImageMat);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Laplacian边缘检测失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Scharr边缘检测
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Scharr()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            var result = basicOperatorService.Scharr(SelectOperation.ImageMat);
            AddDerivedOperation(result.Mat, result.Suffix);
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Scharr边缘检测失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    #endregion

    #region 轮廓检测

    /// <summary>
    /// 寻找轮廓
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task FindContours()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty())
        {
            Growl.ErrorGlobal("图像为空");
            return;
        }

        var confirmed = await ConfirmAlgorithmParametersAsync();
        if (!confirmed) return;

        try
        {
            var result = basicOperatorService.FindContours(
                SelectOperation.ImageMat,
                (RetrievalModes)SelectedRetrievalModes,
                (ContourApproximationModes)SelectedContourApproximationModes,
                SelectedRetrievalModes,
                SelectedContourApproximationModes);

            AddDerivedOperation(result.Mat, result.Suffix);
            if (SelectOperation is not null)
                SelectOperation.ContourObjectList = result.Contours;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("轮廓检测失败" + exception.Message);
        }
    }

    /// <summary>
    /// 通过轮廓绘制边界矩形
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task DrawBoundingRect()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty())
        {
            Growl.ErrorGlobal("图像为空");
            return;
        }

        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(OperationChooseDialog)) is not OperationChooseDialog operationChooseDialog)
        {
            Growl.ErrorGlobal("Operation对话框未找到");
            return;
        }

        operationChooseDialog.OperationsCollection = OperationsCollection;
        await contentDialogService.ShowContentAsync(operationChooseDialog);

        Operation? operation = operationChooseDialog.SelectOperation;
        if (operation is null)
        {
            Growl.ErrorGlobal("未选择Operation");
            return;
        }

        Mat? clone = operation.ImageMat?.Clone();
        if (clone is null)
        {
            return;
        }

        foreach (ContourObject rectangleObject in SelectOperation.ContourObjectList)
        {
            Cv2.Rectangle(clone, rectangleObject.BoundingRect, Scalar.LightGreen);
        }

        Operation op = new Operation();

        op.ImageMat = clone;
        op.ImageName = $"{SelectOperation.ImageName}_DrawingBoundingRect";

        OperationsCollection.Add(op);
        SelectOperation = op;
    }

    #endregion

    #region 辅助命令

    /// <summary>
    /// 删除选中操作
    /// </summary>
    [RelayCommand]
    private Task DeleteOperation()
    {
        if (SelectOperation is null) return Task.CompletedTask;
        OperationsCollection.Remove(SelectOperation);
        SelectOperation = OperationsCollection.LastOrDefault();
        ResetCanvas();
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #region 辅助方法

    private void AddDerivedOperation(Mat dst, string suffix)
    {
        if (SelectOperation is null) return;

        var operation = new Operation
        {
            ImageMat = dst,
            ImageName = $"{SelectOperation.ImageName}_{suffix}"
        };

        OperationsCollection.Add(operation);
        SelectOperation = operation;
    }

    private static void ResetDialogCallbacks(IContentControl dialog)
    {
        dialog.CloseCallback = null;
        dialog.SuccCallback = null;
        dialog.FailCallback = null;
        dialog.CancelCallback = null;
    }

    private async Task<bool> ShowConfirmDialogAsync(IContentControl dialog)
    {
        ResetDialogCallbacks(dialog);

        var tcs = new TaskCompletionSource<bool>();

        await contentDialogService.ShowContentAsync(
            dialog,
            succCallback: _ => tcs.TrySetResult(true),
            cancelCallback: _ => tcs.TrySetResult(false),
            closeCallback: _ => tcs.TrySetResult(false));

        return await tcs.Task;
    }

    private async Task<bool> ConfirmFilterKernelAsync()
    {
        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(FilterKernelDialog)) is not FilterKernelDialog dialog)
        {
            Growl.ErrorGlobal("滤波参数对话框未找到");
            return false;
        }

        dialog.BlockSize = BlockSize;

        var confirmed = await ShowConfirmDialogAsync(dialog);
        if (!confirmed) return false;

        BlockSize = dialog.BlockSize;
        return true;
    }

    private async Task<bool> ConfirmMorphologyAsync()
    {
        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(MorphologyDialog)) is not MorphologyDialog dialog)
        {
            Growl.ErrorGlobal("形态学参数对话框未找到");
            return false;
        }

        dialog.BlockSize = BlockSize;
        dialog.Iterations = Iterations;
        dialog.SelectedKernelShape = SelectedKernelShape;

        var confirmed = await ShowConfirmDialogAsync(dialog);
        if (!confirmed) return false;

        BlockSize = dialog.BlockSize;
        Iterations = dialog.Iterations;
        SelectedKernelShape = dialog.SelectedKernelShape;
        return true;
    }

    private async Task<bool> ConfirmLinearGrayTransformAsync()
    {
        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(LinearGrayTransformDialog)) is not LinearGrayTransformDialog dialog)
        {
            Growl.ErrorGlobal("灰度线性变换参数对话框未找到");
            return false;
        }

        dialog.OutMin = LinearOutMin;
        dialog.OutMax = LinearOutMax;
        dialog.SelectedPresetIndex = LinearPresetIndex;

        // Default input range: use previous selection when available; otherwise fall back to image min/max.
        if (SelectOperation?.ImageMat is { } src && !src.Empty())
        {
            try
            {
                Cv2.MinMaxLoc(src, out double minVal, out double maxVal);
                var minInt = (int)Math.Round(minVal);
                var maxInt = (int)Math.Round(maxVal);
                if (minInt < 0) minInt = 0;
                if (maxInt > 255) maxInt = 255;

                if (LinearInMin == 0 && LinearInMax == 255)
                {
                    dialog.InMin = minInt;
                    dialog.InMax = maxInt;
                }
                else
                {
                    dialog.InMin = LinearInMin;
                    dialog.InMax = LinearInMax;
                }

                using var hist = basicOperatorService.BuildChannelHistogramImage(src, histSize: 256, width: 300, height: 150);
                dialog.HistogramImageSource = hist.ToBitmapSource();
            }
            catch
            {
                // Histogram is optional; selection can still work via numeric boxes.
            }
        }

        var confirmed = await ShowConfirmDialogAsync(dialog);
        if (!confirmed) return false;

        LinearOutMin = dialog.OutMin;
        LinearOutMax = dialog.OutMax;
        LinearInMin = dialog.InMin;
        LinearInMax = dialog.InMax;
        LinearPresetIndex = dialog.SelectedPresetIndex;
        LinearPresetAlpha = dialog.PresetAlpha;
        LinearPresetBeta = dialog.PresetBeta;
        return true;
    }

    private async Task<bool> ConfirmPiecewiseLinearGrayTransformAsync()
    {
        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(PiecewiseLinearGrayTransformDialog)) is not PiecewiseLinearGrayTransformDialog dialog)
        {
            Growl.ErrorGlobal("分段线性灰度变换参数对话框未找到");
            return false;
        }

        var confirmed = await ShowConfirmDialogAsync(dialog);
        if (!confirmed) return false;

        // 从对话框的分段集合中提取参数
        // 暂时保持向后兼容，使用前两个分段的拐点
        if (dialog.Segments.Count >= 2)
        {
            var seg1 = dialog.Segments[0];
            var seg2 = dialog.Segments[1];
            PiecewiseR1 = seg1.InputEnd;
            PiecewiseS1 = seg1.OutputEnd;
            PiecewiseR2 = seg2.InputEnd;
            PiecewiseS2 = seg2.OutputEnd;
        }

        return true;
    }

    private async Task<bool> ConfirmCannyAsync()
    {
        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(CannyDialog)) is not CannyDialog dialog)
        {
            Growl.ErrorGlobal("Canny 参数对话框未找到");
            return false;
        }

        dialog.Threshold1 = ThresholdValue;
        dialog.Threshold2 = ThresholdMaxValue;

        var confirmed = await ShowConfirmDialogAsync(dialog);
        if (!confirmed) return false;

        ThresholdValue = dialog.Threshold1;
        ThresholdMaxValue = dialog.Threshold2;
        return true;
    }

    private async Task<bool> ConfirmAlgorithmParametersAsync()
    {
        if (App.ServiceProvider.GetKeyedService(typeof(IContentControl), nameof(AlgorithmParameterDialog)) is not AlgorithmParameterDialog dialog)
        {
            Growl.ErrorGlobal("算法参数对话框未找到");
            return false;
        }

        dialog.SelectedRetrievalModes = SelectedRetrievalModes;
        dialog.SelectedContourApproximationModes = SelectedContourApproximationModes;

        var confirmed = await ShowConfirmDialogAsync(dialog);
        if (!confirmed) return false;

        SelectedRetrievalModes = dialog.SelectedRetrievalModes;
        SelectedContourApproximationModes = dialog.SelectedContourApproximationModes;

        return true;
    }

    /// <summary>
    /// 重置画布
    /// </summary>
    private void ResetCanvas()
    {
        if (App.ServiceProvider.GetService(typeof(BasicPage)) is BasicPage basicPage)
        {
            basicPage.uiImagePreviewControl.ResizeImageToFit();
        }
    }

    #endregion
}