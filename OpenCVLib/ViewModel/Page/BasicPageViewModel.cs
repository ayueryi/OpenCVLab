using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HandyControl.Controls;

using Yu.UI;

using Microsoft.Win32;

using OpenCVLab.Help;
using OpenCVLab.View.Dialog;
using OpenCVLab.View.Page;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace OpenCVLab.ViewModel.Page;

[Inject]
public partial class BasicPageViewModel : ObservableObject
{
    public BasicPageViewModel(IContentDialogService contentDialogService)
    {
        this.contentDialogService = contentDialogService;
    }

    #region 视图模型

    /// <summary>
    /// 用于显示图像
    /// </summary>
    [ObservableProperty] private BitmapSource? _bitmapSource;

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

    #endregion

    #region 属性

    public IContentDialogService contentDialogService { get; set; }

    public string? Dims => $"{tmpMat?.Dims}";

    public string? ImgSize => $"{tmpMat?.Width}x{tmpMat?.Height}";

    private Mat? tmpMat;

    /// <summary>
    /// 一个Mat的临时属性
    /// </summary>
    public Mat? TempMat
    {
        get => tmpMat;
        set
        {
            tmpMat = value;

            if (tmpMat is not null && !tmpMat.Empty())
            {
                BitmapSource = tmpMat.ToBitmapSource();
                OnPropertyChanged(nameof(Dims));
                OnPropertyChanged(nameof(ImgSize));
            }
        }
    }

    /// <summary>
    /// 图像路径
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

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
            TempMat = new Mat(ofd.FileName, ImreadModes.AnyColor);
            ImagePath = ofd.FileName;

            if (TempMat is null)
            {
                Growl.ErrorGlobal("图像读取失败");
                return Task.CompletedTask;
            }
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal(ImagePath + "图像读取失败" + exception.Message); ;
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
        if (TempMat is null)
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
            TempMat.SaveImage(sfd.FileName);
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
        if (TempMat is null) return Task.CompletedTask;

        Cv2.ImShow("img", TempMat);
        Cv2.WaitKey();

        return Task.CompletedTask;
    }

    /// <summary>
    /// 重读图像
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task RefreshImage()
    {
        TempMat = new Mat(ImagePath, ImreadModes.AnyColor);
        if (TempMat.Empty())
        {
            Growl.ErrorGlobal("图像读取失败");
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    #endregion

    #region 基础操作

    #region 颜色空间转换

    [RelayCommand]
    private async Task ConvertColor()
    {
        if (TempMat is null) return;

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
            Cv2.CvtColor(TempMat, dst, colorConversionCode);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;

        try
        {
            Mat dst = new Mat();
            Cv2.Blur(TempMat, dst, new Size(BlockSize, BlockSize));
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;

        try
        {
            Mat dst = new Mat();
            Cv2.GaussianBlur(TempMat, dst, new Size(BlockSize, BlockSize), 0);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.MedianBlur(TempMat, dst, BlockSize);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.BilateralFilter(TempMat, dst, BlockSize, BlockSize * 2, BlockSize / 2);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.Erode(TempMat, dst, kernal, new Point(-1, -1), Iterations);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.Dilate(TempMat, dst, kernal, new Point(-1, -1), Iterations);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(TempMat, dst, MorphTypes.Open, kernal, new Point(-1, -1), Iterations);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(TempMat, dst, MorphTypes.Close, kernal, new Point(-1, -1), Iterations);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(TempMat, dst, MorphTypes.Gradient, kernal, new Point(-1, -1), Iterations);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(TempMat, dst, MorphTypes.TopHat, kernal, new Point(-1, -1), Iterations);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Mat kernal = Cv2.GetStructuringElement(KernelShape, new Size(BlockSize, BlockSize));
            Cv2.MorphologyEx(TempMat, dst, MorphTypes.BlackHat, kernal, new Point(-1, -1), Iterations);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;

        Enum.TryParse(ThresholdType, out ThresholdTypes thresholdType);

        try
        {
            Mat dst = new Mat();
            Cv2.Threshold(TempMat, dst, ThresholdValue, ThresholdMaxValue, thresholdType);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.AdaptiveThreshold(TempMat, dst, ThresholdMaxValue, AdaptiveThresholdTypes.GaussianC, (ThresholdTypes)SelectedThresholdTypes, BlockSize, 2);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.Threshold(TempMat, dst, 0, ThresholdMaxValue, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            TempMat = dst;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Otsu阈值处理失败" + exception.Message);
            return Task.CompletedTask;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.Canny(TempMat, dst, ThresholdValue, ThresholdMaxValue);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.Sobel(TempMat, dst, MatType.CV_8U, 1, 1);
            TempMat = dst;
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
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.Laplacian(TempMat, dst, MatType.CV_8U);
            TempMat = dst;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("Laplacian边缘检测失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Scharr()
    {
        if (TempMat is null) return Task.CompletedTask;
        try
        {
            Mat dst = new Mat();
            Cv2.Scharr(TempMat, dst, MatType.CV_8U, 1, 1);
            TempMat = dst;
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
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private Task FindContours()
    {
        if (TempMat is null || TempMat.Empty())
        {
            Growl.ErrorGlobal("图像为空");
            return Task.CompletedTask;
        }

        try
        {
            Mat dst = new Mat(TempMat.Width, TempMat.Height, MatType.CV_8UC3);
            Cv2.FindContours(TempMat, out Point[][] contours, out HierarchyIndex[] hierarchy, (RetrievalModes)SelectedRetrievalModes, (ContourApproximationModes)SelectedContourApproximationModes);

            Random random = new Random();
            for (int i = 0; i < contours.Length; i++)
            {
                // 随机生成颜色
                int r = random.Next(0, 256);
                int g = random.Next(0, 256);
                int b = random.Next(0, 256);
                Scalar color = new Scalar(b, g, r);

                Cv2.DrawContours(dst, contours, i, color);
            }

            TempMat = dst;
        }
        catch (Exception exception)
        {
            Growl.ErrorGlobal("轮廓检测失败" + exception.Message);
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #region 辅助方法

    private void ResetCanvas()
    {
        if (App.ServiceProvider.GetService(typeof(BasicPage)) is BasicPage basicPage)
        {
            basicPage.uiImagePreviewControl.ResizeImageToFit();
        }
    }

    #endregion
}