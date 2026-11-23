using UIKit;

namespace FlowNRW;

/// <summary>Native iOS entry point.</summary>
public static class Program
{
    /// <summary>Starts the iOS application.</summary>
    /// <param name="args">Platform launch arguments.</param>
    public static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
