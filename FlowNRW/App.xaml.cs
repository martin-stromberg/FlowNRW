using Microsoft.Extensions.DependencyInjection;
using FlowNRW.Core.Refresh;

namespace FlowNRW;

/// <summary>
/// The FlowNRW MAUI application.
/// </summary>
public partial class App : Application
{
    private readonly AppShell shell;
    private readonly ForegroundState foreground;
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="shell">Composed navigation shell.</param>
    /// <param name="foreground">Shared active-window state.</param>
    public App(AppShell shell, ForegroundState foreground)
    {
        this.shell = shell;
        this.foreground = foreground;
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(shell);
        window.Activated += (_, _) => foreground.IsActive = true;
        window.Deactivated += (_, _) => foreground.IsActive = false;
        window.Destroying += (_, _) => foreground.IsActive = false;
        return window;
    }
}
