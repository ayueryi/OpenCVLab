using System;

namespace Yu.UI;

public sealed class ToastOptions
{
    public ToastSeverity Severity { get; init; } = ToastSeverity.Info;

    public string Message { get; init; } = string.Empty;

    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(3);

    public bool ShowCloseButton { get; init; } = true;
}
