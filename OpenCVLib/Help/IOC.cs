using System.ComponentModel;
using System.Reflection;

using Yu.UI;

using Microsoft.Extensions.DependencyInjection;

namespace OpenCVLab.Help;

/// <summary>
/// 统一的依赖注入
/// </summary>
public static class IocHelper
{
    public static IServiceCollection Injection(this IServiceCollection services)
    {
        #region 特殊注入服务

        // 注册对话框服务
        services.AddSingleton<IContentDialogService, ContentDialogService>();

        #endregion

        // 扫描所有被 Inject 特性标记的类型并注册
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var injectAttribute = type.GetCustomAttribute<InjectAttribute>();
            if (injectAttribute != null)
            {
                switch (injectAttribute.Lifecycle)
                {
                    case Lifecycle.Transient:
                        services.AddTransient(type);
                        break;

                    case Lifecycle.Singleton:
                        services.AddSingleton(type);
                        break;

                    case Lifecycle.Scoped:
                        services.AddScoped(type);
                        break;
                }
            }
        }

        // 扫描所有被 KeyedInject 特性标记的类型并注册
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var keyedInjectAttribute = type.GetCustomAttribute<KeyedInjectAttribute>();
            if (keyedInjectAttribute != null)
            {
                switch (keyedInjectAttribute.Lifecycle)
                {
                    case Lifecycle.Transient:
                        services.AddKeyedTransient(keyedInjectAttribute.TService, keyedInjectAttribute.Key, type);
                        break;

                    case Lifecycle.Singleton:
                        services.AddKeyedSingleton(keyedInjectAttribute.TService, keyedInjectAttribute.Key, type);
                        break;

                    case Lifecycle.Scoped:
                        services.AddKeyedScoped(keyedInjectAttribute.TService, keyedInjectAttribute.Key, type);
                        break;
                }
            }
        }

        return services;
    }
}

/// <summary>
/// IOC容器注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InjectAttribute(Lifecycle lifecycle = Lifecycle.Singleton) : Attribute
{
    public Lifecycle Lifecycle { get; } = lifecycle;
}

/// <summary>
/// IOC容器接口注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class KeyedInjectAttribute(Type TService, string key, Lifecycle lifecycle) : Attribute
{
    public Type TService { get; } = TService;

    public Lifecycle Lifecycle { get; } = lifecycle;

    public string Key { get; } = key;
}


/// <summary>
/// IOC容器注入的生命周期
/// </summary>
public enum Lifecycle
{
    [Description("每次请求都创建新实例")]
    Transient,

    [Description("只创建一次，并共享实例")]
    Singleton,

    [Description("每次请求都创建新实例，但同一个请求共享一个实例")]
    Scoped
}
