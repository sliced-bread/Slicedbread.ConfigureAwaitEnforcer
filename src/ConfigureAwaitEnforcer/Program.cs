namespace ConfigureAwaitEnforcer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Analyser;
    using Deepcode.CommandLine.Parser;

    class Program
    {
        private const string ExcludeFilesSwitch = "excludeFiles";
        private const string StrictFilesSwitch = "strict";
        private static readonly SolutionAnalyser SolutionAnalyser = new SolutionAnalyser();
        private static readonly History.History History = new History.History();

        static async Task Main(string[] args)
        {
            ConsoleWriter.WriteLine("Parsing command line args");
            var arguments = new CommandLineParser().Parse(args);
            if (arguments.Verbs.Length != 1)
            {
                ConsoleWriter.WriteLine($"Expected to get a solution path to build, but received {arguments.Verbs.Length} verbs", ConsoleColor.Red);
                Environment.Exit(1);
            }
            var solutionPath = arguments.Verbs.First();
            var excludeFileNames = GetExcludedFileNamesFromCommandArgs(arguments);
            var strictMode = arguments.Switches.Contains(StrictFilesSwitch);
            if (strictMode)
            {
                ConsoleWriter.WriteLine("Strict mode. Ignoring history - any await without ConfigureAwait will trigger failure", ConsoleColor.Yellow);
            }

            var invalidAwaits = await SolutionAnalyser.GetInvalidAwaitsForSolution(solutionPath, excludeFileNames);

            if (!strictMode)
            {
                ConsoleWriter.WriteLine();
                ConsoleWriter.WriteLine($"Found {invalidAwaits.Count} await(s) without ConfigureAwait");
                ConsoleWriter.WriteLine("Filtering out calls found on first run of this solution..");
                invalidAwaits = History.OnlyNewCallsForSolution(solutionPath, invalidAwaits);
            }

            var colour = invalidAwaits.Any()
                ? ConsoleColor.Red
                : ConsoleColor.Green;

            ConsoleWriter.WriteLine();
            ConsoleWriter.WriteLine($"Found {invalidAwaits.Count} new await call(s) without ConfigureAwait", colour);
            ConsoleWriter.WriteLine();

            WriteInvalidAsyncCallsToOutput(invalidAwaits);

            Environment.Exit(
                    invalidAwaits.Any() ? 1 : 0
            );
        }

        private static List<string> GetExcludedFileNamesFromCommandArgs(CommandLineArguments arguments)
        {
            var excludeList = arguments.Switches.FirstOrDefault(s => s == ExcludeFilesSwitch);
            if (excludeList == null)
                return new List<string>();

            var excludeFileList = arguments.Switch(excludeList).ToList();

            ConsoleWriter.WriteLine($"Excluding files containing {string.Join(", ", excludeFileList)}");
            return excludeFileList;
        }

        private static void WriteInvalidAsyncCallsToOutput(List<InvalidAwait> invalidCalls)
        {
            var groupedByProject = invalidCalls.GroupBy(c => c.ProjectName);
            foreach (var project in groupedByProject)
            {
                var groupedByFile = project.GroupBy(c => c.FileName);

                foreach (var file in groupedByFile)
                {
                    ConsoleWriter.WriteLine($"    {project.Key} - {file.Key}", ConsoleColor.Yellow);
                    foreach (var call in file)
                    {
                        ConsoleWriter.Write($"      {call.LineNumber}: ", ConsoleColor.Red);
                        ConsoleWriter.WriteLine(call.LineText);
                    }
                    ConsoleWriter.WriteLine();
                }
            }
           
        }
    }
}
