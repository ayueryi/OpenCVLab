using System.Windows.Controls;
using System.Windows.Shapes;

using OpenCVLab.Help;
using OpenCVLab.Model;
using OpenCVLab.Model.DedectResult;
using OpenCVLab.ViewModel.Page;

namespace OpenCVLab.View.Page;

[Inject]
public partial class BasicPage : UserControl
{
    public BasicPageViewModel ViewModel { get; }

    public BasicPage(BasicPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    private void DrawBoundingRectCanvas(object sender, System.Windows.RoutedEventArgs e)
    {
        Operation? operation = ViewModel.SelectOperation;
        if (operation is null)
        {
            return;
        }

        foreach (ContourObject rectangleObject in operation.ContourObjectList)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = rectangleObject.BoundingRect.Width,
                Height = rectangleObject.BoundingRect.Height,
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 2
            };

            Canvas.SetLeft(rectangle, rectangleObject.BoundingRect.X);
            Canvas.SetTop(rectangle, rectangleObject.BoundingRect.Y);

            uiImagePreviewControl.Canvas.Children.Add(rectangle);
        }
    }

    private void ClearCanvas(object sender, System.Windows.RoutedEventArgs e)
    {
        uiImagePreviewControl.Canvas.Children.Clear();
    }
}