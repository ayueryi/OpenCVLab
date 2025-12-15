using Yu.UI.Controls.ContentDialog;
using Yu.UI.Controls;

namespace Yu.UI;

/// <summary>
/// 弹窗服务
/// </summary>
public class ContentDialogService : IContentDialogService
{
    private string _dialogIdentifier = "RootDialog";

    public void SetDialogHost(string dialogHost) => _dialogIdentifier = dialogHost;

    public string? GetDialogHost() => _dialogIdentifier;

    private ContentDialogHost GetHostOrThrow()
    {
        if (string.IsNullOrWhiteSpace(_dialogIdentifier))
            throw new InvalidOperationException("The DialogHost was never set.");

        if (!ContentDialogHostRegistry.TryGet(_dialogIdentifier, out var host))
            throw new InvalidOperationException($"Dialog host '{_dialogIdentifier}' was not found.");

        return host;
    }

    public Task ShowSampleTipAsync(string title, string msg, Action? confim, Action? cancel)
    {
        var v = new TipDialog
        {
            TipTitle = title,
            TipContent = msg
        };

        return ShowContentAsync(v,
            succCallback: _ => confim?.Invoke(),
            cancelCallback: _ => cancel?.Invoke());
    }

    public async Task ShowContentAsync(IContentControl? userControl,
                                Action<object>? closeCallback = null,
                                Action<object>? succCallback = null,
                                Action<object>? failCallback = null,
                                Action<object>? cancelCallback = null)
    {
        if (userControl == null)
            throw new ArgumentNullException(nameof(userControl));

        var host = GetHostOrThrow();
        if (host.IsOpen) return;

        userControl.CloseCallback += closeCallback ?? (o => CloseDialog());
        userControl.SuccCallback += succCallback;
        userControl.FailCallback += failCallback;
        userControl.CancelCallback += cancelCallback;
        userControl.SuccCallback += (o) => CloseDialog();
        userControl.CancelCallback += (o) => CloseDialog();

        if (userControl is not System.Windows.Controls.UserControl uc)
            throw new InvalidOperationException("Dialog content must be a WPF UserControl.");

        await host.ShowAsync(uc);
    }

    public Task CloseDialog()
    {
        var host = GetHostOrThrow();
        if (host.IsOpen) host.Close();

        return Task.CompletedTask;
    }
}