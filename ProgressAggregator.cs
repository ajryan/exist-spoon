using System;

namespace ExistSpoon
{
    public static class ProgressAggregator
    {
        public static event Action<string> Published;

        public static void Publish(string message)
        {
            Published?.Invoke(message);
        }
    }
}
