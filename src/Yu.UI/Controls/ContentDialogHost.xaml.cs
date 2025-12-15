using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Yu.UI.Controls;

public partial class ContentDialogHost : UserControl
{
    public static readonly DependencyProperty HostNameProperty =
        DependencyProperty.Register(nameof(HostName), typeof(string), typeof(ContentDialogHost), new PropertyMetadata("RootDialog", OnHostNameChanged));

    public static readonly DependencyProperty CloseOnClickAwayProperty =
        DependencyProperty.Register(nameof(CloseOnClickAway), typeof(bool), typeof(ContentDialogHost), new PropertyMetadata(false));

    public string HostName
    {
        get => (string)GetValue(HostNameProperty);
        set => SetValue(HostNameProperty, value);
    }

    public bool CloseOnClickAway
    {
        get => (bool)GetValue(CloseOnClickAwayProperty);
        set => SetValue(CloseOnClickAwayProperty, value);
    }

    public bool IsOpen { get; private set; }

    public ContentDialogHost()
    {
        InitializeComponent();

        Loaded += (_, _) => ContentDialogHostRegistry.Register(HostName, this);
        Unloaded += (_, _) => ContentDialogHostRegistry.Unregister(HostName, this);

        PART_Overlay.MouseLeftButtonDown += (_, _) =>
        {
            if (CloseOnClickAway) Close();
        };
    }

    public Task ShowAsync(UserControl content)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));

        PART_Content.Content = content;
        PART_Overlay.Visibility = Visibility.Visible;
        PART_Content.Visibility = Visibility.Visible;
        IsOpen = true;

        return Task.CompletedTask;
    }

    public void Close()
    {
        PART_Content.Content = null;
        PART_Content.Visibility = Visibility.Collapsed;
        PART_Overlay.Visibility = Visibility.Collapsed;
        IsOpen = false;
    }

    private static void OnHostNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ContentDialogHost host) return;

        if (e.OldValue is string oldName)
            ContentDialogHostRegistry.Unregister(oldName, host);

        if (e.NewValue is string newName)
            ContentDialogHostRegistry.Register(newName, host);
    }
}
