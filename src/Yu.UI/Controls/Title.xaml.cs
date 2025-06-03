using System.Windows;

namespace Yu.UI.Controls;

public partial class Title
{
    public Title()
    {
        DataContext = this;
        InitializeComponent();
    }

    public static readonly DependencyProperty MainTitleProperty =
        DependencyProperty.Register(
            "MainTitle",
            typeof(string),
            typeof(Title),
            new PropertyMetadata(string.Empty));

    public string MainTitle
    {
        get { return (string)GetValue(MainTitleProperty); }
        set { SetValue(MainTitleProperty, value); }
    }

    public static readonly DependencyProperty SubTitleProperty =
        DependencyProperty.Register(
            "SubTitle",
            typeof(string),
            typeof(Title),
            new PropertyMetadata(string.Empty));

    public string SubTitle
    {
        get { return (string)GetValue(SubTitleProperty); }
        set { SetValue(SubTitleProperty, value); }
    }


    public static readonly DependencyProperty MainTitleSizeProperty =
        DependencyProperty.Register(
            "MainTitleSize",
            typeof(double),
            typeof(Title),
            new PropertyMetadata(22.0));

    public double MainTitleSize
    {
        get { return (double)GetValue(MainTitleSizeProperty); }
        set { SetValue(MainTitleSizeProperty, value); }
    }

    public static readonly DependencyProperty SubTitleSizeProperty =
        DependencyProperty.Register(
            "SubTitleSize",
            typeof(double),
            typeof(Title),
            new PropertyMetadata(12.0));

    public double SubTitleSize
    {
        get { return (double)GetValue(SubTitleSizeProperty); }
        set { SetValue(SubTitleSizeProperty, value); }
    }
}