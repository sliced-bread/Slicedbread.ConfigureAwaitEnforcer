namespace ConfigureAwaitEnforcer
{
    public class InvalidCall
    {
        public string ProjectName { get; }
        public string FileName { get; }
        public string LineText { get; }
        public int LineNumber { get; }

        public InvalidCall(string projectname, string fileName, string lineText, int lineNumber)
        {
            ProjectName = projectname;
            FileName = fileName;
            LineText = lineText;
            LineNumber = lineNumber;
        }
    }
}