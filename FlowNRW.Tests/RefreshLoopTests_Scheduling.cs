using FlowNRW.Core.Refresh;

namespace FlowNRW.Tests;

/// <summary>Deterministic interval, lifecycle and error scheduling without elapsed-time sleeps.</summary>
public sealed class RefreshLoopTests_Scheduling
{
    /// <summary>Changing intervals invalidates old uncooperative delays and starts a full new wait.</summary>
    [Fact]
    public void IntervalChangeRejectsOldDelay()
    {
        var delays = new ControlledRefreshDelay(); var calls = 0; var cancels = 0;
        using var loop = new RefreshLoop(() => { calls++; return Task.CompletedTask; }, () => cancels++, delays.WaitAsync);
        loop.Start(30); loop.Start(120);
        Assert.Equal(1, cancels); Assert.True(delays.Tokens[0].IsCancellationRequested);
        Assert.Equal(TimeSpan.FromSeconds(120), delays.Intervals[1]);
        delays.Pending[0].SetResult(); Assert.Equal(0, calls);
        delays.Pending[1].SetResult(); Assert.Equal(1, calls);
        Assert.Equal(TimeSpan.FromSeconds(120), delays.Intervals[2]);
    }

    /// <summary>No next interval is scheduled until a slow update completes, so no catch-up queue exists.</summary>
    [Fact]
    public void FullDelayBeginsAfterCompletion()
    {
        var delays = new ControlledRefreshDelay(); var request = new TaskCompletionSource(); var calls = 0;
        using var loop = new RefreshLoop(() => { calls++; return request.Task; }, () => { }, delays.WaitAsync);
        loop.Start(60); delays.Pending[0].SetResult();
        Assert.Equal(1, calls); Assert.Single(delays.Pending);
        loop.Start(60); Assert.Single(delays.Pending);
        request.SetResult(); Assert.Equal(2, delays.Pending.Count); Assert.Equal(1, calls);
        Assert.Equal(TimeSpan.FromSeconds(60), delays.Intervals[1]);
        delays.Pending[1].SetResult(); Assert.Equal(2, calls);
    }

    /// <summary>A slow card never blocks another independent loop.</summary>
    [Fact]
    public void IndependentLoopsProgressSeparately()
    {
        var slowDelay = new ControlledRefreshDelay(); var fastDelay = new ControlledRefreshDelay();
        var slowRequest = new TaskCompletionSource(); var fastCalls = 0;
        using var slow = new RefreshLoop(() => slowRequest.Task, () => { }, slowDelay.WaitAsync);
        using var fast = new RefreshLoop(() => { fastCalls++; return Task.CompletedTask; }, () => { }, fastDelay.WaitAsync);
        slow.Start(30); fast.Start(30); slowDelay.Pending[0].SetResult();
        fastDelay.Pending[0].SetResult(); fastDelay.Pending[1].SetResult();
        Assert.Equal(2, fastCalls); Assert.Single(slowDelay.Pending);
        slowRequest.SetResult(); Assert.Equal(2, slowDelay.Pending.Count);
    }

    /// <summary>Failure is observed and a retry waits a whole interval rather than immediately repeating.</summary>
    [Fact]
    public void RefreshFailureWaitsForNextInterval()
    {
        var delays = new ControlledRefreshDelay(); var calls = 0;
        using var loop = new RefreshLoop(() => { calls++; throw new InvalidOperationException("failure"); }, () => { }, delays.WaitAsync);
        loop.Start(30); delays.Pending[0].SetResult();
        Assert.Equal(1, calls); Assert.Equal(2, delays.Pending.Count);
        delays.Pending[1].SetResult(); Assert.Equal(2, calls);
    }

    /// <summary>A failed delay ends safely and permits one explicit restart.</summary>
    [Fact]
    public void FailedDelayDoesNotCreateRetryStorm()
    {
        var delays = new ControlledRefreshDelay(); var calls = 0;
        using var loop = new RefreshLoop(() => { calls++; return Task.CompletedTask; }, () => { }, delays.WaitAsync);
        loop.Start(30); delays.Pending[0].SetException(new IOException("timer failure"));
        Assert.Equal(0, calls); Assert.Single(delays.Pending);
        loop.Start(30); loop.Start(30); Assert.Equal(2, delays.Pending.Count);
        delays.Pending[1].SetResult(); Assert.Equal(1, calls);
    }

    /// <summary>Off and repeated stop are idempotent, and reactivation owns exactly one wait.</summary>
    [Fact]
    public void OffAndReactivationHaveSingleOwnership()
    {
        var delays = new ControlledRefreshDelay(); var cancels = 0;
        var loop = new RefreshLoop(() => Task.CompletedTask, () => cancels++, delays.WaitAsync);
        loop.Start(0); loop.Stop(); Assert.Equal(0, cancels); Assert.Empty(delays.Pending);
        loop.Start(30); loop.Start(0); loop.Stop(); Assert.Equal(1, cancels);
        loop.Start(30); loop.Start(30); Assert.Equal(2, delays.Pending.Count);
        loop.Dispose(); loop.Dispose(); Assert.Equal(2, cancels);
        Assert.Throws<ObjectDisposedException>(() => loop.Start(30));
        foreach (var source in delays.Pending) source.SetResult();
    }

    /// <summary>Native activity signals publish only actual transitions.</summary>
    [Fact]
    public void ForegroundStateNotifiesOnlyTransitions()
    {
        var state = new ForegroundState(); var changes = new List<string?>();
        state.PropertyChanged += (_, args) => changes.Add(args.PropertyName);
        Assert.False(state.IsActive);
        state.IsActive = true; state.IsActive = true; state.IsActive = false; state.IsActive = false;
        Assert.Equal(new[] { nameof(ForegroundState.IsActive), nameof(ForegroundState.IsActive) }, changes);
    }
}
