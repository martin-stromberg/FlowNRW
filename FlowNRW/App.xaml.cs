using Microsoft.Extensions.DependencyInjection;
using FlowNRW.Core.Diagnostics;
using FlowNRW.Core.Refresh;

namespace FlowNRW;

/// <summary>
/// The FlowNRW MAUI application.
/// </summary>
public partial class App : Application
{
    private readonly AppShell shell;
    private readonly RefreshLifecycle lifecycle;
#if UI_TEST_FIXTURES
    private static readonly BindableProperty TextScaleAppliedProperty = BindableProperty.CreateAttached("TextScaleApplied", typeof(bool), typeof(App), false);

    private static bool ApplyTextScaleOnce(VisualElement element)
    {
        if ((bool)element.GetValue(TextScaleAppliedProperty)) return false;
        element.SetValue(TextScaleAppliedProperty, true);
        return true;
    }
#endif
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="shell">Composed navigation shell.</param>
    /// <param name="lifecycle">Shared foreground and background ownership.</param>
    public App(AppShell shell, RefreshLifecycle lifecycle)
    {
        this.shell = shell;
        this.lifecycle = lifecycle;
        AppLog.LoadEnabled();
        AppLog.Write("lifecycle", "app started");
        InitializeComponent();
#if UI_TEST_FIXTURES
        UserAppTheme = Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_THEME") switch { "light" => AppTheme.Light, "dark" => AppTheme.Dark, _ => AppTheme.Unspecified };
        if (Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_TEXT_SCALE") == "150")
        {
            Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping("DesignTextScale", (_, view) => { if (view is Label label && ApplyTextScaleOnce(label)) label.FontSize *= 1.5; });
            Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping("DesignTextScale", (_, view) => { if (view is Button button && ApplyTextScaleOnce(button)) button.FontSize *= 1.5; });
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("DesignTextScale", (_, view) => { if (view is Entry entry && ApplyTextScaleOnce(entry)) entry.FontSize *= 1.5; });
            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("DesignTextScale", (_, view) => { if (view is Picker picker && ApplyTextScaleOnce(picker)) picker.FontSize *= 1.5; });
        }
#endif
    }

    /// <inheritdoc />
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(shell);
#if UI_TEST_FIXTURES
        var pinActive = Environment.GetEnvironmentVariable("FLOWNRW_UI_TEST_PIN_ACTIVE") == "1";
#else
        var pinActive = false;
#endif
        window.Activated += (_, _) => { AppLog.Write("lifecycle", "window activated"); lifecycle.SetActive(true); };
        window.Deactivated += (_, _) => { if (!pinActive) { AppLog.Write("lifecycle", "window deactivated"); lifecycle.SetActive(false); } };
        window.Stopped += (_, _) => { if (!pinActive) { AppLog.Write("lifecycle", "window stopped"); lifecycle.SetActive(false); } };
        window.Resumed += (_, _) => { AppLog.Write("lifecycle", "window resumed"); lifecycle.SetActive(true); };
        window.Destroying += (_, _) => { AppLog.Write("lifecycle", "window destroying"); lifecycle.SetActive(false); };
        return window;
    }
}
