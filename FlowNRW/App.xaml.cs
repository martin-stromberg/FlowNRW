using Microsoft.Extensions.DependencyInjection;

namespace FlowNRW;

/// <summary>
/// The FlowNRW MAUI application.
/// </summary>
public partial class App : Application
{
    private readonly AppShell shell;
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="shell">Composed navigation shell.</param>
    public App(AppShell shell)
    {
        this.shell = shell;
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(shell);
    }
}
