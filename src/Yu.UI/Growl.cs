using System;
using System.Windows;

namespace Yu.UI;

/// <summary>
/// HandyControl.Growl-compatible API that renders as a custom bottom-right toast.
/// </summary>
public static class Growl
{
    public static void InfoGlobal(string message) => ToastService.Show(new ToastOptions { Severity = ToastSeverity.Info, Message = message });

    public static void SuccessGlobal(string message) => ToastService.Show(new ToastOptions { Severity = ToastSeverity.Success, Message = message });

    public static void WarningGlobal(string message) => ToastService.Show(new ToastOptions { Severity = ToastSeverity.Warning, Message = message });

    public static void ErrorGlobal(string message) => ToastService.Show(new ToastOptions { Severity = ToastSeverity.Error, Message = message });

    public static void ClearGlobal() => ToastService.Clear();

    public static void Info(string message) => InfoGlobal(message);

    public static void Success(string message) => SuccessGlobal(message);

    public static void Warning(string message) => WarningGlobal(message);

    public static void Error(string message) => ErrorGlobal(message);
}
