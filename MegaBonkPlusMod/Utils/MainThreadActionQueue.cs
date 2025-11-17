using System;
using System.Collections.Concurrent;

namespace MegaBonkPlusMod.Utils;

public static class MainThreadActionQueue
{
    private static readonly ConcurrentQueue<Action> _queue = new();

    public static void QueueAction(Action action)
    {
        _queue.Enqueue(action);
    }

    public static void ExecuteAll()
    {
        while (_queue.TryDequeue(out var action)) action?.Invoke();
    }
}