namespace Yu.UI;

/// <summary>
/// ContentDialogService Design Base MaterialDesignFramework
/// </summary>
public interface IContentDialogService
{
    void SetDialogHost(string hostName);

    string? GetDialogHost();

    Task ShowSampleTipAsync(string title, string msg, Action? confim = null, Action? cancel = null);

    Task ShowContentAsync(IContentControl? userControl, Action<object>? closeCallback = null, Action<object>? succCallback = null, Action<object>? failCallback = null, Action<object>? cancelCallback = null);

    Task CloseDialog();
}