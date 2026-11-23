using Foundation;
using UIKit;

namespace FlowNRW;

/// <summary>Scene-Delegate fuer den ab iOS-SDK 27 erzwungenen UIScene-Lifecycle.</summary>
[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
    /// <inheritdoc />
    public override void DidEnterBackground(UIScene scene)
    {
        base.DidEnterBackground(scene);
        IosBackgroundRefresh.Schedule();
    }
}
