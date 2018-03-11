namespace ConfigureAwaitEnforcer.Analyser
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class DocumentAnalyser
    {
        public async Task<IEnumerable<InvalidCall>> GetInvalidAwaitCallsAsync(Document document)
        {
            var syntaxTree = await document.GetSyntaxRootAsync();

            var awaits = syntaxTree.DescendantNodes().OfType<AwaitExpressionSyntax>().ToList();
            if (!awaits.Any())
                return new List<InvalidCall>();

            var model = await document.GetSemanticModelAsync();
            var text = await document.GetTextAsync();

            var list = new List<InvalidCall>();
            foreach (var awaitExpression in awaits)
            {
                var typeName = model.GetTypeInfo(awaitExpression.Expression).Type.Name;
                if (typeName == nameof(ConfiguredTaskAwaitable))
                    continue;

                if (typeName == "dynamic")
                {
                    // We can't rely on the type any more, so check the syntax tree
                    //  to see if the code tries to access the ConfigureAwait member on the Task
                    var memberAccessExpression = awaitExpression
                        .Expression
                        .DescendantNodes()
                        .OfType<MemberAccessExpressionSyntax>()
                        .FirstOrDefault();

                    if (memberAccessExpression != null &&
                        memberAccessExpression.Name.Identifier.Text == "ConfigureAwait")
                        continue;
                }

                var line = text.Lines.GetLineFromPosition(awaitExpression.SpanStart);

                // Have we flagged this line already in this run? Some lines may trigger twice
                // (e.g. 'await Task.Run(async () => await MethodAsync());');
                if (list.Any(x => x.LineNumber == line.LineNumber + 1))
                    continue;

                list.Add(new InvalidCall(document.Project.Name, document.Name, line.ToString().Trim(), line.LineNumber + 1));
            }

            return list;
        }
    }
}