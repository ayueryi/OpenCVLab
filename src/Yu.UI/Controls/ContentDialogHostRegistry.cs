using System.Collections.Concurrent;

namespace Yu.UI.Controls;

internal static class ContentDialogHostRegistry
{
    private static readonly ConcurrentDictionary<string, ContentDialogHost> Hosts = new();

    public static void Register(string hostName, ContentDialogHost host) => Hosts[hostName] = host;

    public static void Unregister(string hostName, ContentDialogHost host)
    {
        if (Hosts.TryGetValue(hostName, out var existing) && ReferenceEquals(existing, host))
        {
            Hosts.TryRemove(hostName, out _);
        }
    }

    public static bool TryGet(string hostName, out ContentDialogHost host) => Hosts.TryGetValue(hostName, out host!);
}
