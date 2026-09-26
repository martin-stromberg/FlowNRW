using Foundation;
using UIKit;

namespace FlowNRW;

/// <summary>Native iOS application delegate.</summary>
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <inheritdoc />
    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        IosBackgroundRefresh.Register();
        return base.FinishedLaunching(application, launchOptions);
    }

    /// <inheritdoc />
    public override void DidEnterBackground(UIApplication application)
    {
        base.DidEnterBackground(application);
        IosBackgroundRefresh.Schedule();
    }

    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
