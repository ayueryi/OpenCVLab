using OpenCVLab.Model.DedectResult;
using OpenCvSharp;

namespace OpenCVLab.Services.Operators;

public sealed record FindContoursResult(Mat Mat, List<ContourObject> Contours, string Suffix);
