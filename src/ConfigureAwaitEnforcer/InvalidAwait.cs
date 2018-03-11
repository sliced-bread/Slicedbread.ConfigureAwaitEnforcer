namespace ConfigureAwaitEnforcer
{
    public class InvalidAwait
    {
        public string ProjectName { get; }
        public string FileName { get; }
        public string LineText { get; }
        public int LineNumber { get; }

        public InvalidAwait(string projectname, string fileName, string lineText, int lineNumber)
        {
            ProjectName = projectname;
            FileName = fileName;
            LineText = lineText;
            LineNumber = lineNumber;
        }
    }
}