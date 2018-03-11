namespace ConfigureAwaitEnforcer.History
{
    public class HistoricalInvalidCall
    {
        public string ProjectName { get; }
        public string FileName { get; }
        public int LineNumber { get; }

        public HistoricalInvalidCall(string projectName, string fileName, int lineNumber)
        {
            ProjectName = projectName;
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }
}