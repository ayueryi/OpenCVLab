using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HandyControl.Controls;

using Microsoft.Win32;

using OpenCVLab.Help;
using OpenCVLab.Model;
using OpenCVLab.Model.DedectResult;
using OpenCVLab.View.Dialog;
using OpenCVLab.View.Page;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using Yu.UI;

namespace OpenCVLab.ViewModel.Page;

[Inject]
public partial class BasicPageViewModel : ObservableObject
{
    public BasicPageViewModel(IContentDialogService contentDialogService)
    {
        this.contentDialogService = contentDialogService;
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

        // 定义直方图参数
        int histSize = 256;
        Rangef histRange = new Rangef(0, 256);
        bool uniform = true;
        bool accumulate = false;

        // 根据通道数处理
        int channelCount = src.Channels();
        Mat[] hists = new Mat[channelCount];

        // 计算每个通道的直方图
        for (int c = 0; c < channelCount; c++)
        {
            // 分离通道
            Mat channel = new Mat();
            Cv2.ExtractChannel(src, channel, c);

            // 计算直方图
            Mat[] images = { channel };
            hists[c] = new Mat();
            int[] channels = { 0 };
            Mat mask = new Mat();
            Cv2.CalcHist(images, channels, mask, hists[c], 1, new int[] { histSize }, new Rangef[] { histRange }, uniform, accumulate);

            channel.Dispose();
        }

        // 创建直方图图像
        int hist_w = 300;
        int hist_h = 150;
        int bin_w = (int)Math.Round((double)hist_w / histSize);

        Mat histImage = new Mat(hist_h, hist_w, MatType.CV_8UC3, Scalar.All(0));

        // 绘制每个通道的直方图
        for (int c = 0; c < channelCount; c++)
        {
            // 归一化直方图
            Cv2.Normalize(hists[c], hists[c], 0, histImage.Rows, NormTypes.MinMax, -1, new Mat());

            Scalar color;
            switch (c)
            {
                case 0:
                    color = Scalar.Blue;
                    break;
                case 1:
                    color = Scalar.Green;
                    break;
                case 2:
                    color = Scalar.Red;
                    break;
                default:
                    color = Scalar.White;
                    break;
            }

            for (int i = 1; i < histSize; i++)
            {
                Cv2.Line(histImage, new Point(bin_w * (i - 1), hist_h - (int)Math.Round(hists[c].Get<float>(i - 1))),
                                 new Point(bin_w * (i), hist_h - (int)Math.Round(hists[c].Get<float>(i))),
                                 color, 2, LineTypes.AntiAlias, 0);
            }
        }

        HistImageSource = histImage.ToBitmapSource();

        // 释放资源
        foreach (var hist in hists)
        {
            hist.Dispose();
        }
        histImage.Dispose();
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
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal(ofd.FileName + "图像读取失败" + exception.Message); ;
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

    /// <summary>
    /// 显示图像
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task ShowImage()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty())
            return Task.CompletedTask;

        Cv2.ImShow("img", SelectOperation.ImageMat);
        Cv2.WaitKey();

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

        await contentDialogService.ShowContentAsync(colorConversionDialog);

        Enum.TryParse(colorConversionDialog.ColorConversionName, out ColorConversionCodes colorConversionCode);
        try
        {
            Mat dst = new Mat();
            Cv2.CvtColor(SelectOperation.ImageMat, dst, colorConversionCode);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_{colorConversionDialog.ColorConversionName}";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("颜色空间转换失败" + exception.Message);
            return;
        }

        return;
    }

    #endregion

    #region 滤波

    /// <summary>
    /// 归一化滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task NormalizedBlockFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;

        try
        {
            Mat dst = new Mat();
            Cv2.Blur(SelectOperation.ImageMat, dst, new Size(BlockSize, BlockSize));

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Blur";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("归一化滤波失败" + exception.Message);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 高斯滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task GaussianFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;

        try
        {
            Mat dst = new Mat();
            Cv2.GaussianBlur(SelectOperation.ImageMat, dst, new Size(BlockSize, BlockSize), 0);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_GaussianBlur";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("高斯滤波失败" + exception.Message);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 中值滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task MedianFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.MedianBlur(SelectOperation.ImageMat, dst, BlockSize);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_MedianBlur";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("中值滤波失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 双边滤波
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task BilateralFilter()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.BilateralFilter(SelectOperation.ImageMat, dst, BlockSize, BlockSize * 2, BlockSize / 2);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_BilateralBlur";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("双边滤波失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    #endregion

    #region 基础形态学

    /// <summary>
    /// 腐蚀
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Erode()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.Erode(SelectOperation.ImageMat, dst, kernal, new Point(-1, -1), Iterations);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Erode";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("腐蚀失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 膨胀
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Dilate()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.Dilate(SelectOperation.ImageMat, dst, kernal, new Point(-1, -1), Iterations);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Dilate";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("膨胀失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    #endregion

    #region 高阶形态学

    /// <summary>
    /// 开运算
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task MorphologyExOpen()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(SelectOperation.ImageMat, dst, MorphTypes.Open, kernal, new Point(-1, -1), Iterations);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_MorphologyExOpen";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[开运算]操作失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 闭运算
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task MorphologyExClose()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(SelectOperation.ImageMat, dst, MorphTypes.Close, kernal, new Point(-1, -1), Iterations);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_MorphologyExClose";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[闭运算]操作失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 梯度
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task MorphologyExGradient()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(SelectOperation.ImageMat, dst, MorphTypes.Gradient, kernal, new Point(-1, -1), Iterations);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_MorphologyExGradient";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[梯度]操作失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 顶帽
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task MorphologyExTopHat()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(SelectOperation.ImageMat, dst, MorphTypes.TopHat, kernal, new Point(-1, -1), Iterations);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_MorphologyExTopHat";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[顶帽]操作失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 黑帽
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task MorphologyExBlackHat()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(SelectOperation.ImageMat, dst, MorphTypes.BlackHat, kernal, new Point(-1, -1), Iterations);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_MorphologyExBlackHat";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("形态学[黑帽]操作失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }
    #endregion

    #endregion

    #region 阈值处理

    /// <summary>
    /// 简单阈值处理
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Threshold(string ThresholdType)
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;

        Enum.TryParse(ThresholdType, out ThresholdTypes thresholdType);

        try
        {
            Mat dst = new Mat();
            Cv2.Threshold(SelectOperation.ImageMat, dst, ThresholdValue, ThresholdMaxValue, thresholdType);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Threshold_{ThresholdType}";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("阈值处理失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 自适应阈值处理
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task AdaptiveThreshold()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.AdaptiveThreshold(SelectOperation.ImageMat, dst, ThresholdMaxValue, AdaptiveThresholdTypes.GaussianC, (ThresholdTypes)SelectedThresholdTypes, BlockSize, 2);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_AdaptiveThreshold";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("自适应阈值处理失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Otsu阈值处理
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task OtsuThreshold()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;

        try
        {
            Mat dst = new Mat();
            Cv2.Threshold(SelectOperation.ImageMat, dst, 0, ThresholdMaxValue, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_OtsuThreshold";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Otsu阈值处理失败" + exception.Message);
            return Task.CompletedTask;
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
            // 直方图均衡化
            Mat dst = new Mat();
            Cv2.EqualizeHist(src, dst);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation!.ImageName}_EqualizeHist";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
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
            // 创建CLAHE对象
            CLAHE clahe = Cv2.CreateCLAHE(40, new Size(8, 8));

            // 应用CLAHE
            Mat dst = new Mat();
            clahe.Apply(src, dst);

            Operation operation = new Operation();
            operation.ImageMat = dst;

            operation.ImageName = $"{SelectOperation!.ImageName}_CreateCLAHE";
            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal($"{exception.Message}");
        }

        return Task.CompletedTask;
    }

    #endregion

    #region 边缘检测

    /// <summary>
    /// Canny边缘检测
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task Canny()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty()) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.Canny(SelectOperation.ImageMat, dst, ThresholdValue, ThresholdMaxValue);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Canny";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Canny边缘检测失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
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
            Mat dst = new Mat();
            Cv2.Sobel(SelectOperation.ImageMat, dst, MatType.CV_8U, 1, 1);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Sobel";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
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
            Mat dst = new Mat();
            Cv2.Laplacian(SelectOperation.ImageMat, dst, MatType.CV_8U);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Laplacian";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
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
            Mat dst = new Mat();
            Cv2.Scharr(SelectOperation.ImageMat, dst, MatType.CV_8U, 1, 1);

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_Scharr";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
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
    private Task FindContours()
    {
        if (SelectOperation is null || SelectOperation.ImageMat is null || SelectOperation.ImageMat.Empty())
        {
            Growl.ErrorGlobal("图像为空");
            return Task.CompletedTask;
        }

        try
        {
            List<ContourObject> contourObjects = new List<ContourObject>();

            Mat dst = new Mat(SelectOperation.ImageMat.Width, SelectOperation.ImageMat.Height, MatType.CV_8UC3);
            Cv2.FindContours(SelectOperation.ImageMat, out Point[][] contours, out HierarchyIndex[] hierarchy, (RetrievalModes)SelectedRetrievalModes, (ContourApproximationModes)SelectedContourApproximationModes);

            Random random = new Random();
            for (int i = 0; i < contours.Length; i++)
            {
                // 随机生成颜色
                int r = random.Next(0, 256);
                int g = random.Next(0, 256);
                int b = random.Next(0, 256);
                Scalar color = new Scalar(b, g, r);

                Cv2.DrawContours(dst, contours, i, color);

                // 创建一个新的 ContourObject 实例
                var contourObject = new ContourObject
                {
                    Mask = dst.Clone(),
                    ContourPoints = [contours[i]],
                    Area = Cv2.ContourArea(contours[i]),
                    Perimeter = Cv2.ArcLength(contours[i], true),
                    BoundingRect = Cv2.BoundingRect(contours[i])
                };

                contourObjects.Add(contourObject);
            }

            Operation operation = new Operation();
            operation.ImageMat = dst;
            operation.ImageName = $"{SelectOperation.ImageName}_FindContours";

            OperationsCollection.Add(operation);
            SelectOperation = operation;
            SelectOperation.ContourObjectList = contourObjects;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("轮廓检测失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
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

        return;
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