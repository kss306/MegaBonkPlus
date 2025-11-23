using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BonkersLib.Utils;

namespace BonkersLib.Core;

public static class MainThreadDispatcher
{
    private static readonly ConcurrentQueue<Action> _executionQueue = new();
    private static int _mainThreadId;

    public static void Initialize()
    {
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public static T Evaluate<T>(Func<T> action)
    {
        if (Thread.CurrentThread.ManagedThreadId == _mainThreadId)
        {
            return action();
        }

        var tcs = new TaskCompletionSource<T>();

        _executionQueue.Enqueue(() =>
        {
            try
            {
                var result = action();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task.Result;
    }

    public static void Enqueue(Action action) => _executionQueue.Enqueue(action);

    internal static void Update()
    {
        while (_executionQueue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"Dispatcher Error: {ex}");
            }
        }
    }
}