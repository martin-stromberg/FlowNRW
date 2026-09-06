using FlowNRW.Core;

namespace FlowNRW;

/// <summary>
/// Start page showing the click counter.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly ClickCounter counter = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        CounterBtn.Text = counter.Click();
        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
