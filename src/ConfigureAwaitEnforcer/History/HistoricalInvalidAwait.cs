namespace ConfigureAwaitEnforcer.History
{
    public class HistoricalInvalidAwait
    {
        public string ProjectName { get; }
        public string FileName { get; }
        public int LineNumber { get; }

        public HistoricalInvalidAwait(string projectName, string fileName, int lineNumber)
        {
            ProjectName = projectName;
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }
}