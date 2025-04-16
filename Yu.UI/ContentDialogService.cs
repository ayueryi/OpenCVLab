using Yu.UI.Controls.ContentDialog;

using MaterialDesignThemes.Wpf;

namespace Yu.UI;

/// <summary>
/// 弹窗服务
/// </summary>
public class ContentDialogService : IContentDialogService
{
    private string _dialogIdentifier = "RootDialog";

    public void SetDialogHost(string dialogHost) => _dialogIdentifier = dialogHost;

    public string? GetDialogHost() => _dialogIdentifier;

    public Task ShowSampleTipAsync(string title, string msg, Action? confim, Action? cancel)
    {
        if (_dialogIdentifier == null)
            throw new InvalidOperationException("The DialogHost was never set.");

        if (DialogHost.IsDialogOpen(_dialogIdentifier))
            return Task.CompletedTask;

        TipDialog v = new TipDialog();
        v.SuccCallback = confim;
        v.FailCallback = cancel;

        Action finlly = () => { _ = CloseDialog(); };
        v.SuccCallback += finlly;
        v.FailCallback += finlly;

        v.TipTitle = title;
        v.TipContent = msg;

        DialogHost.Show(v, _dialogIdentifier, v.DialogClosingEventHandler);

        return Task.CompletedTask;
    }

    public async Task ShowContentAsync(IContentControl? userControl,
                                Action<object>? closeCallback = null,
                                Action<object>? succCallback = null,
                                Action<object>? failCallback = null,
                                Action<object>? cancelCallback = null)
    {
        if (userControl == null)
            throw new ArgumentNullException(nameof(userControl));

        if (_dialogIdentifier == null)
            throw new InvalidOperationException("The DialogHost was never set.");

        if (DialogHost.IsDialogOpen(_dialogIdentifier))
            return;

        userControl.CloseCallback += closeCallback ?? (o => CloseDialog());
        userControl.SuccCallback += succCallback;
        userControl.FailCallback += failCallback;
        userControl.CancelCallback += cancelCallback;
        userControl.SuccCallback += (o) => CloseDialog();
        userControl.CancelCallback += (o) => CloseDialog();
        await DialogHost.Show(userControl, _dialogIdentifier);
    }

    public Task CloseDialog()
    {
        if (DialogHost.IsDialogOpen(_dialogIdentifier))
            DialogHost.Close(_dialogIdentifier);

        return Task.CompletedTask;
    }
}