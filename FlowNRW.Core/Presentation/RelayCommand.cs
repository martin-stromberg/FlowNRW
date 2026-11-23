using System.Windows.Input;

namespace FlowNRW.Core.Presentation;

/// <summary>Synchronous binding action.</summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> action;
    private readonly Func<bool> enabled;
    /// <summary>Creates an action.</summary>
    /// <param name="action">Action receiving the selected item.</param>
    /// <param name="enabled">Availability predicate.</param>
    public RelayCommand(Action<object?> action, Func<bool>? enabled = null) { this.action = action; this.enabled = enabled ?? (() => true); }
    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;
    /// <inheritdoc />
    public bool CanExecute(object? parameter) => enabled();
    /// <inheritdoc />
    public void Execute(object? parameter) { if (CanExecute(parameter)) action(parameter); }
    /// <summary>Updates availability.</summary>
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
