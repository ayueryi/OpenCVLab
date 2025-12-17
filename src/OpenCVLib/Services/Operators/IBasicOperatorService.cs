using OpenCVLab.Model.DedectResult;
using OpenCvSharp;

namespace OpenCVLab.Services.Operators;

public interface IBasicOperatorService
{
    Mat BuildChannelHistogramImage(Mat src, int histSize = 256, int width = 300, int height = 150);

    OperatorResult ConvertColor(Mat src, ColorConversionCodes code, string requestedName);

    OperatorResult Blur(Mat src, int blockSize);
    OperatorResult GaussianBlur(Mat src, int blockSize);
    OperatorResult MedianBlur(Mat src, int blockSize);
    OperatorResult BilateralFilter(Mat src, int diameter);

    OperatorResult Erode(Mat src, MorphShapes shape, int blockSize, int iterations, int selectedKernelShape);
    OperatorResult Dilate(Mat src, MorphShapes shape, int blockSize, int iterations, int selectedKernelShape);

    OperatorResult MorphologyEx(Mat src, MorphTypes morphType, MorphShapes shape, int blockSize, int iterations, int selectedKernelShape);

    OperatorResult Threshold(Mat src, ThresholdTypes thresholdType, int thresholdValue, int maxValue, string thresholdTypeName);
    OperatorResult AdaptiveThreshold(Mat src, AdaptiveThresholdTypes adaptiveType, ThresholdTypes thresholdType, int blockSize, int maxValue, int selectedThresholdTypes);
    OperatorResult OtsuThreshold(Mat src, int maxValue);
    OperatorResult TriangleThreshold(Mat src, int maxValue);

    OperatorResult EqualizeHist(Mat src);
    OperatorResult CreateClahe(Mat src, double clipLimit = 40, int tileGridSize = 8);

    OperatorResult LinearGrayTransform(Mat src, int outMin, int outMax);
    OperatorResult LinearGrayTransform(Mat src, int inMin, int inMax, int outMin, int outMax);
    OperatorResult LinearAlphaBetaTransform(Mat src, double alpha, double beta);
    OperatorResult PiecewiseLinearGrayTransform(Mat src, int r1, int s1, int r2, int s2);
    OperatorResult GammaTransform(Mat src, double gamma);

    OperatorResult Canny(Mat src, int threshold1, int threshold2);
    OperatorResult Sobel(Mat src);
    OperatorResult Laplacian(Mat src);
    OperatorResult Scharr(Mat src);

    OperatorResult Invert(Mat src);

    FindContoursResult FindContours(Mat src, RetrievalModes retrievalMode, ContourApproximationModes approximationMode, int selectedRetrievalModes, int selectedContourApproximationModes);
}
