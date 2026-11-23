using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowNRW.Core.Presentation;

/// <summary>Observable presentation state.</summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Notifies a binding of a changed value.</summary>
    /// <param name="name">Property name.</param>
    protected void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
