using OpenCVLab.Model.DedectResult;

using OpenCvSharp;

namespace OpenCVLab.Services.Operators;

public sealed class BasicOperatorService : IBasicOperatorService
{
    public Mat BuildChannelHistogramImage(Mat src, int histSize = 256, int width = 300, int height = 150)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (src.Empty()) throw new ArgumentException("src is empty", nameof(src));
        if (histSize <= 0) throw new ArgumentOutOfRangeException(nameof(histSize));
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        var histRange = new Rangef(0, 256);
        var channelCount = src.Channels();
        var hists = new Mat[channelCount];

        try
        {
            for (var c = 0; c < channelCount; c++)
            {
                using var channel = new Mat();
                Cv2.ExtractChannel(src, channel, c);

                var images = new[] { channel };
                hists[c] = new Mat();
                Cv2.CalcHist(images, new[] { 0 }, new Mat(), hists[c], 1, new[] { histSize }, new[] { histRange }, true, false);
            }

            var binWidth = (int)Math.Round((double)width / histSize);
            var histImage = new Mat(height, width, MatType.CV_8UC3, Scalar.All(0));

            for (var c = 0; c < channelCount; c++)
            {
                Cv2.Normalize(hists[c], hists[c], 0, histImage.Rows, NormTypes.MinMax, -1, new Mat());

                var color = c switch
                {
                    0 => Scalar.Blue,
                    1 => Scalar.Green,
                    2 => Scalar.Red,
                    _ => Scalar.White
                };

                for (var i = 1; i < histSize; i++)
                {
                    Cv2.Line(
                        histImage,
                        new Point(binWidth * (i - 1), height - (int)Math.Round(hists[c].Get<float>(i - 1))),
                        new Point(binWidth * i, height - (int)Math.Round(hists[c].Get<float>(i))),
                        color,
                        2,
                        LineTypes.AntiAlias,
                        0);
                }
            }

            return histImage;
        }
        finally
        {
            foreach (var hist in hists)
            {
                hist?.Dispose();
            }
        }
    }

    public OperatorResult ConvertColor(Mat src, ColorConversionCodes code, string requestedName)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.CvtColor(src, dst, code);
        return new OperatorResult(dst, requestedName);
    }

    public OperatorResult Blur(Mat src, int blockSize)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Blur(src, dst, new Size(blockSize, blockSize));
        return new OperatorResult(dst, $"Blur_k{blockSize}");
    }

    public OperatorResult GaussianBlur(Mat src, int blockSize)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.GaussianBlur(src, dst, new Size(blockSize, blockSize), 0);
        return new OperatorResult(dst, $"GaussianBlur_k{blockSize}");
    }

    public OperatorResult MedianBlur(Mat src, int blockSize)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.MedianBlur(src, dst, blockSize);
        return new OperatorResult(dst, $"MedianBlur_k{blockSize}");
    }

    public OperatorResult BilateralFilter(Mat src, int diameter)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.BilateralFilter(src, dst, diameter, diameter * 2, diameter / 2.0);
        return new OperatorResult(dst, $"BilateralBlur_d{diameter}");
    }

    public OperatorResult Erode(Mat src, MorphShapes shape, int blockSize, int iterations, int selectedKernelShape)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        using var kernel = Cv2.GetStructuringElement(shape, new Size(blockSize, blockSize));
        Cv2.Erode(src, dst, kernel, new Point(-1, -1), iterations);
        return new OperatorResult(dst, $"Erode_k{blockSize}_it{iterations}_s{selectedKernelShape}");
    }

    public OperatorResult Dilate(Mat src, MorphShapes shape, int blockSize, int iterations, int selectedKernelShape)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        using var kernel = Cv2.GetStructuringElement(shape, new Size(blockSize, blockSize));
        Cv2.Dilate(src, dst, kernel, new Point(-1, -1), iterations);
        return new OperatorResult(dst, $"Dilate_k{blockSize}_it{iterations}_s{selectedKernelShape}");
    }

    public OperatorResult MorphologyEx(Mat src, MorphTypes morphType, MorphShapes shape, int blockSize, int iterations, int selectedKernelShape)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        using var kernel = Cv2.GetStructuringElement(shape, new Size(blockSize, blockSize));
        Cv2.MorphologyEx(src, dst, morphType, kernel, new Point(-1, -1), iterations);

        var opName = morphType switch
        {
            MorphTypes.Open => "MorphologyExOpen",
            MorphTypes.Close => "MorphologyExClose",
            MorphTypes.Gradient => "MorphologyExGradient",
            MorphTypes.TopHat => "MorphologyExTopHat",
            MorphTypes.BlackHat => "MorphologyExBlackHat",
            _ => $"MorphologyEx{morphType}"
        };

        return new OperatorResult(dst, $"{opName}_k{blockSize}_it{iterations}_s{selectedKernelShape}");
    }

    public OperatorResult Threshold(Mat src, ThresholdTypes thresholdType, int thresholdValue, int maxValue, string thresholdTypeName)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Threshold(src, dst, thresholdValue, maxValue, thresholdType);
        return new OperatorResult(dst, $"Threshold_{thresholdTypeName}_th{thresholdValue}_max{maxValue}");
    }

    public OperatorResult AdaptiveThreshold(Mat src, AdaptiveThresholdTypes adaptiveType, ThresholdTypes thresholdType, int blockSize, int maxValue, int selectedThresholdTypes)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.AdaptiveThreshold(src, dst, maxValue, adaptiveType, thresholdType, blockSize, 2);
        return new OperatorResult(dst, $"AdaptiveThreshold_k{blockSize}_tt{selectedThresholdTypes}_max{maxValue}");
    }

    public OperatorResult OtsuThreshold(Mat src, int maxValue)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Threshold(src, dst, 0, maxValue, ThresholdTypes.Binary | ThresholdTypes.Otsu);
        return new OperatorResult(dst, $"OtsuThreshold_max{maxValue}");
    }

    public OperatorResult TriangleThreshold(Mat src, int maxValue)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Threshold(src, dst, 0, maxValue, ThresholdTypes.Binary | ThresholdTypes.Triangle);
        return new OperatorResult(dst, $"TriangleThreshold_max{maxValue}");
    }

    public OperatorResult EqualizeHist(Mat src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.EqualizeHist(src, dst);
        return new OperatorResult(dst, "EqualizeHist");
    }

    public OperatorResult CreateClahe(Mat src, double clipLimit = 40, int tileGridSize = 8)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        using var clahe = Cv2.CreateCLAHE(clipLimit, new Size(tileGridSize, tileGridSize));
        var dst = new Mat();
        clahe.Apply(src, dst);
        return new OperatorResult(dst, "CreateCLAHE");
    }

    public OperatorResult LinearGrayTransform(Mat src, int outMin, int outMax)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        double minVal;
        double maxVal;
        Cv2.MinMaxLoc(src, out minVal, out maxVal);
        if (Math.Abs(maxVal - minVal) < 1e-12)
            throw new InvalidOperationException("Image range is constant");

        return LinearGrayTransform(src, (int)Math.Round(minVal), (int)Math.Round(maxVal), outMin, outMax);
    }

    public OperatorResult LinearGrayTransform(Mat src, int inMin, int inMax, int outMin, int outMax)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (inMin < 0 || inMin > 255) throw new ArgumentOutOfRangeException(nameof(inMin));
        if (inMax < 0 || inMax > 255) throw new ArgumentOutOfRangeException(nameof(inMax));
        if (outMin < 0 || outMin > 255) throw new ArgumentOutOfRangeException(nameof(outMin));
        if (outMax < 0 || outMax > 255) throw new ArgumentOutOfRangeException(nameof(outMax));
        if (inMin >= inMax) throw new InvalidOperationException("Input range is invalid");

        var alpha = (outMax - outMin) / (double)(inMax - inMin);
        var beta = outMin - (inMin * alpha);

        using var clipped = new Mat();
        Cv2.Min(src, new Scalar(inMax), clipped);
        Cv2.Max(clipped, new Scalar(inMin), clipped);

        var dst = new Mat();
        clipped.ConvertTo(dst, MatType.CV_8UC1, alpha, beta);
        return new OperatorResult(dst, $"LinearGrayTransform_min{outMin}_max{outMax}");
    }

    public OperatorResult LinearAlphaBetaTransform(Mat src, double alpha, double beta)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        var dst = new Mat();
        // Saturate cast into 8-bit.
        src.ConvertTo(dst, MatType.CV_8UC1, alpha, beta);
        return new OperatorResult(dst, $"LinearAlphaBeta_a{alpha}_b{beta}");
    }

    public OperatorResult PiecewiseLinearGrayTransform(Mat src, int r1, int s1, int r2, int s2)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        var lutBytes = BuildPiecewiseLut(r1, s1, r2, s2);
        using var lut = Mat.FromArray(lutBytes);

        var dst = new Mat();
        Cv2.LUT(src, lut, dst);
        return new OperatorResult(dst, $"PiecewiseLinearGrayTransform_r1{r1}_s1{s1}_r2{r2}_s2{s2}");
    }

    public OperatorResult Canny(Mat src, int threshold1, int threshold2)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Canny(src, dst, threshold1, threshold2);
        return new OperatorResult(dst, $"Canny_t1{threshold1}_t2{threshold2}");
    }

    public OperatorResult Sobel(Mat src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Sobel(src, dst, MatType.CV_8U, 1, 1);
        return new OperatorResult(dst, "Sobel");
    }

    public OperatorResult Laplacian(Mat src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Laplacian(src, dst, MatType.CV_8U);
        return new OperatorResult(dst, "Laplacian");
    }

    public OperatorResult Scharr(Mat src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        var dst = new Mat();
        Cv2.Scharr(src, dst, MatType.CV_8U, 1, 1);
        return new OperatorResult(dst, "Scharr");
    }

    public OperatorResult Invert(Mat src)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        var dst = new Mat();
        Cv2.BitwiseNot(src, dst);
        return new OperatorResult(dst, "Invert");
    }

    public FindContoursResult FindContours(Mat src, RetrievalModes retrievalMode, ContourApproximationModes approximationMode, int selectedRetrievalModes, int selectedContourApproximationModes)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));

        Cv2.FindContours(src, out Point[][] contours, out HierarchyIndex[] _, retrievalMode, approximationMode);

        var dst = new Mat(src.Width, src.Height, MatType.CV_8UC3);
        var contourObjects = new List<ContourObject>(contours.Length);

        var random = new Random();
        for (var i = 0; i < contours.Length; i++)
        {
            var color = new Scalar(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
            Cv2.DrawContours(dst, contours, i, color);

            contourObjects.Add(new ContourObject
            {
                Mask = dst.Clone(),
                ContourPoints = [contours[i]],
                Area = Cv2.ContourArea(contours[i]),
                Perimeter = Cv2.ArcLength(contours[i], true),
                BoundingRect = Cv2.BoundingRect(contours[i])
            });
        }

        return new FindContoursResult(dst, contourObjects, $"FindContours_rm{selectedRetrievalModes}_am{selectedContourApproximationModes}");
    }

    private static byte[] BuildPiecewiseLut(int r1, int s1, int r2, int s2)
    {
        static byte Clamp(double v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return (byte)Math.Round(v);
        }

        var lut = new byte[256];

        for (var i = 0; i < 256; i++)
        {
            double v;

            if (i <= r1)
                v = r1 == 0 ? s1 : (double)s1 * i / r1;
            else if (i <= r2)
                v = (double)s1 + (double)(s2 - s1) * (i - r1) / (r2 - r1);
            else
                v = r2 == 255 ? 255 : (double)s2 + (double)(255 - s2) * (i - r2) / (255 - r2);

            lut[i] = Clamp(v);
        }

        return lut;
    }
}
