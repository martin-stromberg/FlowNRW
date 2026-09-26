using Microsoft.Extensions.DependencyInjection;
using FlowNRW.Core.Refresh;

namespace FlowNRW;

/// <summary>
/// The FlowNRW MAUI application.
/// </summary>
public partial class App : Application
{
    private readonly AppShell shell;
    private readonly RefreshLifecycle lifecycle;
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="shell">Composed navigation shell.</param>
    /// <param name="lifecycle">Shared foreground and background ownership.</param>
    public App(AppShell shell, RefreshLifecycle lifecycle)
    {
        this.shell = shell;
        this.lifecycle = lifecycle;
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(shell);
        window.Activated += (_, _) => lifecycle.SetActive(true);
        window.Deactivated += (_, _) => lifecycle.SetActive(false);
        window.Stopped += (_, _) => lifecycle.SetActive(false);
        window.Resumed += (_, _) => lifecycle.SetActive(true);
        window.Destroying += (_, _) => lifecycle.SetActive(false);
        return window;
    }
}
