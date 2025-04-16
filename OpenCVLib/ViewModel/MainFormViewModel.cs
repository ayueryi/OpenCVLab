using CommunityToolkit.Mvvm.ComponentModel;

using OpenCVLab.Help;
using OpenCVLab.View.Page;

namespace OpenCVLab.ViewModel;

[Inject]
public partial class MainFormViewModel : ObservableObject
{
    public MainFormViewModel(BasicPage basicPage)
    {
        BasicPage = basicPage;
    }

    #region 属性

    #region 页面

    public BasicPage BasicPage { get; set; }

    #endregion

    #endregion
}