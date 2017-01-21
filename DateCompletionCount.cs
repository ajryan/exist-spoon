using System;

namespace ExistSpoon
{
    public class DateCompletionCount
    {
        public DateTime Date { get; }
        public int CompletedCount { get; }

        public DateCompletionCount(DateTime date, int completedCount)
        {
            Date = date;
            CompletedCount = completedCount;
        }
    }
}
