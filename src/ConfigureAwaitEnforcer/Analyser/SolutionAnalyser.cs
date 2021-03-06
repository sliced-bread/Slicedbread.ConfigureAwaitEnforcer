﻿namespace ConfigureAwaitEnforcer.Analyser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.Build.Locator;

    public class SolutionAnalyser
    {
        private static readonly DocumentAnalyser DocumentAnalyser = new DocumentAnalyser();

        public async Task<List<InvalidAwait>> GetInvalidAwaitsForSolution(string solutionPath, IEnumerable<string> excludeFilesContaining)
        {
            ConsoleWriter.WriteLine($"Loading solution {solutionPath}");
            ConsoleWriter.WriteLine();
            var solution = await GetSolutionByPathAsync(solutionPath);

            ConsoleWriter.WriteLine("Analysing:");
            var invalidAwaits = new List<InvalidAwait>();
            foreach (var project in solution.Projects)
            {
                var docsToParse = project
                    .Documents
                    .Where(d => excludeFilesContaining.All(e => d.Name.IndexOf(e, StringComparison.OrdinalIgnoreCase) < 0))
                    .ToList();

                ConsoleWriter.WriteLine($"  {project.Name} - {docsToParse.Count} documents");
                foreach (var document in docsToParse)
                {
                    invalidAwaits.AddRange(await DocumentAnalyser.GetInvalidAwaitCallsAsync(document));
                }
            }

            return invalidAwaits;
        }

        private static Task<Solution> GetSolutionByPathAsync(string solutionPath)
        {
            ConsoleWriter.WriteLine("Locating MSBuild");
            MSBuildLocator.RegisterDefaults();

            ConsoleWriter.WriteLine("MSBuild registered, creating Workspace");
            var msWorkspace = MSBuildWorkspace.Create();
            msWorkspace.WorkspaceFailed += (s, e) => ConsoleWriter.WriteLine(e.Diagnostic.Message, ConsoleColor.Red);

            return msWorkspace.OpenSolutionAsync(solutionPath);
        }
    }
}