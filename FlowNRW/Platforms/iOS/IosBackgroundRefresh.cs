using BackgroundTasks;
using CoreFoundation;
using Foundation;
using FlowNRW.Core.Refresh;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace FlowNRW;

/// <summary>Registers one opportunistic iOS refresh without promising a periodic background interval.</summary>
internal static class IosBackgroundRefresh
{
    private const string Identifier = "de.martinstromberg.flownrw.refresh";
    private static bool registered;

    /// <summary>Registers the handler before application launch finishes.</summary>
    internal static void Register()
    {
        if (registered) return;
        registered = BGTaskScheduler.Shared.Register(Identifier, DispatchQueue.MainQueue, Run);
    }

    /// <summary>Replaces this application's single pending request, subject to OS availability.</summary>
    internal static void Schedule()
    {
        if (!registered) return;
        BGTaskScheduler.Shared.Cancel(Identifier);
        if (UIApplication.SharedApplication.BackgroundRefreshStatus != UIBackgroundRefreshStatus.Available) return;
        using var request = new BGAppRefreshTaskRequest(Identifier)
        {
            EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(15 * 60)
        };
#pragma warning disable CA1422 // out-NSError overload is obsolete since iOS 27; the completion-handler overload is unavailable on older bindings
        BGTaskScheduler.Shared.Submit(request, out var error);
        error?.Dispose();
#pragma warning restore CA1422
    }

    private static async void Run(BGTask task)
    {
        using var expiration = new CancellationTokenSource();
        var completed = false;
        var success = false;
        task.ExpirationHandler = () => MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!completed) expiration.Cancel();
        });
        try
        {
            var services = IPlatformApplication.Current?.Services;
            if (services is not null)
                success = await services.GetRequiredService<RefreshLifecycle>().RunBackgroundAsync(expiration.Token);
        }
        catch (Exception) { success = false; }
        finally
        {
            completed = true;
            task.ExpirationHandler = null;
            task.SetTaskCompleted(success && !expiration.IsCancellationRequested);
            Schedule();
        }
    }
}
