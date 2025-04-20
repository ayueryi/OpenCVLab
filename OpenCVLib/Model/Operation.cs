using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Model.DedectResult;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace OpenCVLab.Model;

public partial class Operation : ObservableObject
{
    [ObservableProperty] private string imageName = string.Empty;

    [ObservableProperty] private Mat? imageMat;

    public BitmapSource? BitmapSource => ImageMat?.ToBitmapSource();

    #region Dedects

    public List<ContourObject> ContourObjectList { get; set; } = [];

    #endregion

    #region 视图属性

    public string Name
    {
        get
        {
            if (ImageName.Length > 15)
                return ImageName.Substring(ImageName.Length - 15, 15);
            else return ImageName;
        }
    }

    public int Channels => ImageMat?.Channels() ?? -1;

    #endregion
}
