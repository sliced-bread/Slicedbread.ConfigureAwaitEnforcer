﻿namespace ConfigureAwaitEnforcer.Analyser
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class DocumentAnalyser
    {
        public async Task<IEnumerable<InvalidAwait>> GetInvalidAwaitCallsAsync(Document document)
        {
            var syntaxTree = await document.GetSyntaxRootAsync();

            var awaits = syntaxTree.DescendantNodes().OfType<AwaitExpressionSyntax>().ToList();
            if (!awaits.Any())
                return new List<InvalidAwait>();

            var model = await document.GetSemanticModelAsync();
            var text = await document.GetTextAsync();

            var list = new List<InvalidAwait>();
            foreach (var awaitExpression in awaits)
            {
                var typeName = model.GetTypeInfo(awaitExpression.Expression).Type.Name;
                if (typeName == nameof(ConfiguredTaskAwaitable))
                    continue;

                if (typeName == "dynamic")
                {
                    // We can't rely on the type any more, so check the syntax tree
                    //  to see if the code tries to access the ConfigureAwait member on the Task
                    var memberAccessExpressions = awaitExpression
                        .Expression
                        .DescendantNodes()
                        .OfType<MemberAccessExpressionSyntax>();

                    if (memberAccessExpressions.Any(e => e.Name.Identifier.Text == "ConfigureAwait"))
                        continue;
                }

                var line = text.Lines.GetLineFromPosition(awaitExpression.SpanStart);

                // Have we flagged this line already in this run? Some lines may trigger twice
                // (e.g. 'await Task.Run(async () => await MethodAsync());');
                var itemAlreadyInList = list.Any(x =>
                    x.ProjectName == document.Project.Name &&
                    x.FileName == document.Name &&
                    x.LineNumber == line.LineNumber + 1
                );

                if (itemAlreadyInList)
                    continue;

                list.Add(new InvalidAwait(
                    document.Project.Name,
                    document.Name,
                    line.ToString().Trim(),
                    line.LineNumber + 1)
                );
            }

            return list;
        }
    }
}