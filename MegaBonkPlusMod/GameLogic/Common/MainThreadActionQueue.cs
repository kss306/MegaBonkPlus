using System;
using System.Collections.Concurrent;

namespace MegaBonkPlusMod.GameLogic.Common
{
    public static class MainThreadActionQueue
    {
        private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        
        public static void QueueAction(Action action)
        {
            _queue.Enqueue(action);
        }
        
        public static void ExecuteAll()
        {
            while (_queue.TryDequeue(out Action action))
            {
                action?.Invoke();
            }
        }
    }
}