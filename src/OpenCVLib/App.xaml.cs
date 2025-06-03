using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using OpenCVLab.Help;
using OpenCVLab.View;

namespace OpenCVLab;

public partial class App
{
    /// <summary>
    /// IOC容器
    /// </summary>
    public static ServiceProvider ServiceProvider = new ServiceCollection().Injection().BuildServiceProvider();

    /// <summary>
    ///     获取注册的服务
    /// </summary>l
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null" />.</returns>
    public static T? GetService<T>() where T : class => ServiceProvider.GetRequiredService(typeof(T)) as T;

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        MainForm? form = GetService<MainForm>();
        if (form is not null) form.Show();
        else MessageBox.Show("form load error");
    }
}