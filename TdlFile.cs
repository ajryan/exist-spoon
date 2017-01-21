using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private static readonly Regex DoneRegex = new Regex(
            @"DONEDATESTRING=""(?<date>\d\d\d\d-\d\d-\d\d|\d\d?/\d\d?/\d\d\d\d)",
            RegexOptions.Compiled);

        public List<DateCompletionCount> PerseCompletions()
        {
            string tdlText        = File.ReadAllText(_path);
            string archiveTdlText = File.ReadAllText(_path.Replace(".tdl", ".done.tdl"));

            var mainMatches    = DoneRegex.Matches(tdlText).Cast<Match>();
            var archiveMatches = DoneRegex.Matches(archiveTdlText).Cast<Match>();

            var matchGroups = mainMatches.Union(archiveMatches).GroupBy(match =>
            {
                string dateString = match.Groups["date"].Value;

                DateTime doneDate;

                DateTime.TryParseExact(
                    dateString,
                    new[] { "yyyy-MM-dd", "M/d/yyyy" },
                    new CultureInfo("en-US"),
                    DateTimeStyles.None,
                    out doneDate);
                
                return doneDate;
            });

            var completions = matchGroups.Where(aGroup => DateTime.Today.Subtract(aGroup.Key).Days < 7)
                                         .Select(aGroup => new DateCompletionCount(aGroup.Key, aGroup.Count()))
                                         .ToList();

            foreach (var completion in completions)
                ProgressAggregator.Publish($"Found {completion.CompletedCount} complete on {completion.Date}.");

            return completions;
        }
    }
}
