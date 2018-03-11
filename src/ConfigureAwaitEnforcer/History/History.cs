namespace ConfigureAwaitEnforcer.History
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    public class History
    {
        public List<InvalidCall> OnlyNewCallsForSolution(string solutionPath, List<InvalidCall> invalidAwaits)
        {
            // If this is the first run, ignore all of these
            var dataFolder = $"{AppContext.BaseDirectory}\\data\\";
            var solutionHistoryFile = dataFolder + Path.GetFileNameWithoutExtension(solutionPath);

            if (!File.Exists(solutionHistoryFile))
            {
                ConsoleWriter.WriteLine("This is the first run, ignoring all invalid awaits", ConsoleColor.Yellow);

                var history = invalidAwaits.Select(i => new HistoricalInvalidCall(i.ProjectName, i.FileName, i.LineNumber));

                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                File.WriteAllText(solutionHistoryFile, JsonConvert.SerializeObject(history));
                invalidAwaits.Clear();
            }
            else
            {
                var historyText = File.ReadAllText(solutionHistoryFile);
                var history = JsonConvert.DeserializeObject<IList<HistoricalInvalidCall>>(historyText);

                invalidAwaits = invalidAwaits.Where(i =>
                        !history.Any(h => h.ProjectName == i.ProjectName && 
                                          h.FileName == i.FileName &&
                                          h.LineNumber == i.LineNumber)
                    ).ToList();
            }

            return invalidAwaits;
        }
    }
}