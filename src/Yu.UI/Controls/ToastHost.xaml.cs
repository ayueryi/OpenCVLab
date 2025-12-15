using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Yu.UI.Controls;

public partial class ToastHost : UserControl, IToastHost
{
    public static readonly DependencyProperty HostNameProperty =
        DependencyProperty.Register(nameof(HostName), typeof(string), typeof(ToastHost), new PropertyMetadata("Global", OnHostNameChanged));

    public string HostName
    {
        get => (string)GetValue(HostNameProperty);
        set => SetValue(HostNameProperty, value);
    }

    public ObservableCollection<ToastViewModel> Toasts { get; } = new();

    public ToastHost()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += (_, _) => ToastService.RegisterHost(HostName, this);
        Unloaded += (_, _) => ToastService.UnregisterHost(HostName, this);
    }

    public void Show(ToastOptions options)
    {
        var duration = options.Duration <= TimeSpan.Zero ? TimeSpan.FromSeconds(3) : options.Duration;

        var toast = new ToastViewModel(options);
        Toasts.Insert(0, toast);

        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = duration };

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            RemoveToast(toast);
        };

        timer.Start();
    }

    public void Clear() => Toasts.Clear();

    private void CloseToast_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ToastViewModel toast })
        {
            RemoveToast(toast);
        }
    }

    private void RemoveToast(ToastViewModel toast)
    {
        if (Toasts.Contains(toast)) Toasts.Remove(toast);
    }

    private static void OnHostNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ToastHost host) return;

        if (e.OldValue is string oldName)
            ToastService.UnregisterHost(oldName, host);

        if (e.NewValue is string newName)
            ToastService.RegisterHost(newName, host);
    }
}

public sealed class ToastViewModel
{
    public ToastSeverity Severity { get; }

    public string Message { get; }

    public bool ShowCloseButton { get; }

    public ToastViewModel(ToastOptions options)
    {
        Severity = options.Severity;
        Message = options.Message;
        ShowCloseButton = options.ShowCloseButton;
    }
}
