using OpenCvSharp;

namespace OpenCVLab.Model.DedectResult;

public class ContourObject
{
    public Mat? Mask { get; set; }
    public Point[][]? ContourPoints { get; set; }
    public double Area { get; set; }
    public double Perimeter { get; set; }
    public Rect BoundingRect { get; set; }
}