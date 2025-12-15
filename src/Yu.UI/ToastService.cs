using System;
using System.Collections.Concurrent;
using System.Windows;

namespace Yu.UI;

public static class ToastService
{
    private static readonly ConcurrentDictionary<string, IToastHost> Hosts = new();

    public static string DefaultHost { get; set; } = "Global";

    public static void RegisterHost(string hostName, IToastHost host)
    {
        if (string.IsNullOrWhiteSpace(hostName)) throw new ArgumentException("Host name is required", nameof(hostName));
        Hosts[hostName] = host;
    }

    public static void UnregisterHost(string hostName, IToastHost host)
    {
        if (string.IsNullOrWhiteSpace(hostName)) return;
        if (Hosts.TryGetValue(hostName, out var existing) && ReferenceEquals(existing, host))
        {
            Hosts.TryRemove(hostName, out _);
        }
    }

    public static void Show(ToastOptions options) => Show(options, DefaultHost);

    public static void Show(ToastOptions options, string hostName)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        void Dispatch()
        {
            if (Hosts.TryGetValue(hostName, out var host))
            {
                host.Show(options);
                return;
            }

            // Fallback: if there is no host yet, fail softly.
            // This avoids crashes during startup or unit tests.
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
            Dispatch();
        else
            dispatcher.Invoke(Dispatch);
    }

    public static void Clear() => Clear(DefaultHost);

    public static void Clear(string hostName)
    {
        void Dispatch()
        {
            if (Hosts.TryGetValue(hostName, out var host)) host.Clear();
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
            Dispatch();
        else
            dispatcher.Invoke(Dispatch);
    }
}
