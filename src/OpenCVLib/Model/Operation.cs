using System.Windows.Media.Imaging;
using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Model.DedectResult;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace OpenCVLab.Model;

public partial class Operation : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string imageName = string.Empty;

    [ObservableProperty] private Mat? imageMat;

    public BitmapSource? BitmapSource => ImageMat?.ToBitmapSource();

    #region Dedects

    public List<ContourObject> ContourObjectList { get; set; } = [];

    #endregion

    #region 视图属性

    public string DisplayName => BuildDisplayName(ImageName);

    public string Name
    {
        get
        {
            // Kept for backward compatibility with existing bindings.
            // Prefer DisplayName for user-friendly text.
            return DisplayName;
        }
    }

    public int Channels => ImageMat?.Channels() ?? -1;

    private static string BuildDisplayName(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName))
            return string.Empty;

        // Try to find the last known operation suffix in the chained ImageName.
        // Example: "foo_GaussianBlur_k5" -> "GaussianBlur (k=5)"
        var knownKeys = new[]
        {
            "PiecewiseLinearGrayTransform",
            "LinearGrayTransform",
            "MorphologyExBlackHat",
            "MorphologyExGradient",
            "MorphologyExTopHat",
            "MorphologyExClose",
            "MorphologyExOpen",
            "AdaptiveThreshold",
            "OtsuThreshold",
            "GaussianBlur",
            "MedianBlur",
            "BilateralBlur",
            "EqualizeHist",
            "CreateCLAHE",
            "FindContours",
            "DrawingBoundingRect",
            "Threshold",
            "Laplacian",
            "Scharr",
            "Sobel",
            "Canny",
            "Dilate",
            "Erode",
            "Blur",
        };

        foreach (var key in knownKeys)
        {
            var marker = "_" + key;
            var index = imageName.LastIndexOf(marker, StringComparison.Ordinal);
            if (index < 0)
                continue;

            var tail = imageName[(index + 1)..];
            return Ellipsize(FormatOperationTail(tail), 34);
        }

        // Fallback: treat the last token as a possible color conversion code (e.g. BGR2GRAY).
        var lastToken = imageName.Split('_').LastOrDefault();
        if (!string.IsNullOrWhiteSpace(lastToken) && IsLikelyColorConversion(lastToken))
            return Ellipsize($"ColorConvert ({lastToken})", 34);

        return Ellipsize(imageName, 34);
    }

    private static string FormatOperationTail(string tail)
    {
        // Tail is like: "Erode_k3_it2_sRect" or "Threshold_Binary_th127_max255".
        var parts = tail.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return tail;

        var op = parts[0];

        // Common no-parameter ops
        if (parts.Length == 1)
        {
            return op switch
            {
                "DrawingBoundingRect" => "DrawBoundingRect",
                _ => op
            };
        }

        // Parse key/value-ish tokens.
        string? kernel = null;
        string? iterations = null;
        string? shape = null;
        string? thresholdType = null;
        string? threshold = null;
        string? min = null;
        string? max = null;
        string? r1 = null;
        string? s1 = null;
        string? r2 = null;
        string? s2 = null;
        string? t1 = null;
        string? t2 = null;
        string? retrievalMode = null;
        string? approxMode = null;
        string? adaptiveType = null;
        string? diameter = null;

        foreach (var token in parts.Skip(1))
        {
            if (token.StartsWith("k", StringComparison.Ordinal) && token.Length > 1)
                kernel = token[1..];
            else if (token.StartsWith("it", StringComparison.Ordinal) && token.Length > 2)
                iterations = token[2..];
            else if (token.StartsWith("s", StringComparison.Ordinal) && token.Length > 1)
                shape = token[1..];
            else if (token.StartsWith("th", StringComparison.Ordinal) && token.Length > 2)
                threshold = token[2..];
            else if (token.StartsWith("min", StringComparison.Ordinal) && token.Length > 3)
                min = token[3..];
            else if (token.StartsWith("max", StringComparison.Ordinal) && token.Length > 3)
                max = token[3..];
            else if (token.StartsWith("r1", StringComparison.Ordinal) && token.Length > 2)
                r1 = token[2..];
            else if (token.StartsWith("s1", StringComparison.Ordinal) && token.Length > 2)
                s1 = token[2..];
            else if (token.StartsWith("r2", StringComparison.Ordinal) && token.Length > 2)
                r2 = token[2..];
            else if (token.StartsWith("s2", StringComparison.Ordinal) && token.Length > 2)
                s2 = token[2..];
            else if (token.StartsWith("t1", StringComparison.Ordinal) && token.Length > 2)
                t1 = token[2..];
            else if (token.StartsWith("t2", StringComparison.Ordinal) && token.Length > 2)
                t2 = token[2..];
            else if (token.StartsWith("rm", StringComparison.Ordinal) && token.Length > 2)
                retrievalMode = token[2..];
            else if (token.StartsWith("am", StringComparison.Ordinal) && token.Length > 2)
                approxMode = token[2..];
            else if (token.StartsWith("tt", StringComparison.Ordinal) && token.Length > 2)
                adaptiveType = token[2..];
            else if (token.StartsWith("d", StringComparison.Ordinal) && token.Length > 1)
                diameter = token[1..];
            else
                thresholdType ??= token;
        }

        var sb = new StringBuilder();
        sb.Append(op switch
        {
            "DrawingBoundingRect" => "DrawBoundingRect",
            _ => op
        });

        var args = new List<string>(6);
        if (!string.IsNullOrWhiteSpace(thresholdType) && op == "Threshold")
            args.Add(thresholdType);

        if (!string.IsNullOrWhiteSpace(kernel))
            args.Add($"k={kernel}");
        if (!string.IsNullOrWhiteSpace(diameter))
            args.Add($"d={diameter}");
        if (!string.IsNullOrWhiteSpace(iterations))
            args.Add($"it={iterations}");
        if (!string.IsNullOrWhiteSpace(shape))
            args.Add($"shape={shape}");
        if (!string.IsNullOrWhiteSpace(threshold))
            args.Add($"th={threshold}");
        if (!string.IsNullOrWhiteSpace(min))
            args.Add(op == "LinearGrayTransform" ? $"outMin={min}" : $"min={min}");
        if (!string.IsNullOrWhiteSpace(max))
            args.Add(op == "LinearGrayTransform" ? $"outMax={max}" : $"max={max}");
        if (op == "PiecewiseLinearGrayTransform")
        {
            if (!string.IsNullOrWhiteSpace(r1)) args.Add($"r1={r1}");
            if (!string.IsNullOrWhiteSpace(s1)) args.Add($"s1={s1}");
            if (!string.IsNullOrWhiteSpace(r2)) args.Add($"r2={r2}");
            if (!string.IsNullOrWhiteSpace(s2)) args.Add($"s2={s2}");
        }
        if (!string.IsNullOrWhiteSpace(adaptiveType))
            args.Add($"type={adaptiveType}");
        if (!string.IsNullOrWhiteSpace(t1))
            args.Add($"t1={t1}");
        if (!string.IsNullOrWhiteSpace(t2))
            args.Add($"t2={t2}");
        if (!string.IsNullOrWhiteSpace(retrievalMode))
            args.Add($"rm={retrievalMode}");
        if (!string.IsNullOrWhiteSpace(approxMode))
            args.Add($"am={approxMode}");

        if (args.Count > 0)
        {
            sb.Append(" (");
            sb.Append(string.Join(", ", args));
            sb.Append(')');
        }

        return sb.ToString();
    }

    private static bool IsLikelyColorConversion(string token)
    {
        if (token.Length < 4 || token.Length > 24)
            return false;
        if (!token.Contains('2', StringComparison.Ordinal))
            return false;

        foreach (var ch in token)
        {
            if (!char.IsLetterOrDigit(ch))
                return false;
        }

        return true;
    }

    private static string Ellipsize(string text, int maxLen)
    {
        if (string.IsNullOrEmpty(text) || maxLen <= 0)
            return string.Empty;
        if (text.Length <= maxLen)
            return text;
        if (maxLen <= 1)
            return text[..1];
        return text[..(maxLen - 1)] + "…";
    }

    #endregion
}
