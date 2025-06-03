using System.Windows;
using System.Windows.Input;

using HandyControl.Controls;

using Yu.UI;

using OpenCVLab.Help;
using OpenCVLab.ViewModel;

namespace OpenCVLab.View;

[Inject]
public partial class MainForm
{
    public MainFormViewModel ViewModel { get; }

    private IContentDialogService ContentDialogService { get; }

    public MainForm(MainFormViewModel viewModel, IContentDialogService contentDialogService)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        ContentDialogService = contentDialogService;
        ContentDialogService.SetDialogHost(nameof(RootDialog));
    }

    #region 窗体方法

    private void TitlePanel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton is MouseButton.Left)
        {
            if (e.ClickCount is 2) Sw_Maxus_Click(sender, e);

            DragMove();
        }
    }

    private void Sw_Closeus_Click(object sender, RoutedEventArgs e)
    {
        Growl.ClearGlobal();

        Close();
    }

    private void Sw_Minus_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Sw_Maxus_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    #endregion
}