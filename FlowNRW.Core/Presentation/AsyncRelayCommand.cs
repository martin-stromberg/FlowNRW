using System.Windows.Input;

namespace FlowNRW.Core.Presentation;

/// <summary>Awaited action with observed failures and duplicate-start protection.</summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> action;
    private readonly Func<bool> enabled;
    private readonly Action<Exception> failure;
    private bool running;
    private long revision;
    /// <summary>Creates an asynchronous action.</summary>
    /// <param name="action">Operation.</param>
    /// <param name="enabled">Availability predicate.</param>
    /// <param name="failure">Failure observer.</param>
    public AsyncRelayCommand(Func<Task> action, Func<bool> enabled, Action<Exception> failure) { this.action = action; this.enabled = enabled; this.failure = failure; }
    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
    /// <inheritdoc />
    public bool CanExecute(object? parameter) => !running && enabled();
    /// <inheritdoc />
    public async void Execute(object? parameter) => await ExecuteAsync();
    /// <summary>Runs the action once while observing errors.</summary>
    /// <returns>Completion.</returns>
    public async Task ExecuteAsync()
    {
        if (!CanExecute(null)) return;
        var version = ++revision;
        running = true; Refresh();
        try { await action(); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (version == revision) failure(ex); }
        finally { if (version == revision) { running = false; Refresh(); } }
    }
    /// <summary>Releases a superseded operation without waiting for an uncooperative service.</summary>
    public void InvalidateExecution()
    {
        revision++;
        running = false;
        Refresh();
    }
    /// <summary>Updates availability.</summary>
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
