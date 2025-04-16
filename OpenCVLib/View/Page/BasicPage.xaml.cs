using System.Windows.Controls;

using OpenCVLab.Help;
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
}