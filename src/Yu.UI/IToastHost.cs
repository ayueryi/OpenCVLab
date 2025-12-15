namespace Yu.UI;

public interface IToastHost
{
    void Show(ToastOptions options);

    void Clear();
}
