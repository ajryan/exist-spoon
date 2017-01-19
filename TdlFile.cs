using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ExistSpoon
{
    public class TdlFile
    {
        private readonly string _path;

        public TdlFile(string path)
        {
            _path = path;
        }

        private static readonly int Year  = DateTime.Today.Year;
        private static readonly int Month = DateTime.Today.Month;
        private static readonly int Day   = DateTime.Today.Day;

        private static readonly Regex DoneRegex = new Regex(
            $"DONEDATESTRING=\"({Year}-{Month:00}-{Day:00}|{Month}/{Day}/{Year})",
            RegexOptions.Compiled);

        public int ParseCompletedCount()
        {
            string tdlText     = File.ReadAllText(_path);
            string doneTdlText = File.ReadAllText(_path.Replace(".tdl", ".done.tdl"));

            int mainMatchCount    = DoneRegex.Matches(tdlText).Count;
            int archiveMatchCount = DoneRegex.Matches(doneTdlText).Count;

            ProgressAggregator.Publish($"Found {mainMatchCount} complete in main task list, {archiveMatchCount} in archive.");

            return mainMatchCount + archiveMatchCount;
        }
    }
}
