using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VersionedCollectionsGenerator
{
    [Generator]
    public sealed class VersionedCollectionsGenerator : ISourceGenerator
    {
        private const string AttributeText = """
using System;

namespace VersionedCollectionsGenerator
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class GenerateVersionedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal sealed class CloneForVersionedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal sealed class RewriteForVersionedAttribute : Attribute
    {
    }
}
""";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("GenerateVersionedAttribute.g.cs", SourceText.From(AttributeText, Encoding.UTF8));

            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            foreach (var candidate in receiver.Candidates)
            {
                var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(candidate) as INamedTypeSymbol;
                if (symbol is null)
                {
                    continue;
                }

                if (!symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "VersionedCollectionsGenerator.GenerateVersionedAttribute"))
                {
                    continue;
                }

                string originalName = symbol.Name;
                string newName = $"{originalName}Versioned";

                var partials = GetAllPartials(context.Compilation, originalName);
                int index = 0;
                foreach (var partial in partials)
                {
                    var tree = partial.SyntaxTree;
                    var root = tree.GetRoot();
                    var rewriter = new RenameRewriter(originalName, newName);
                    var newRoot = rewriter.Visit(root);
                    var text = newRoot.ToFullString()
                        .Replace("#if WITH_VERSIONING", string.Empty)
                        .Replace("#endif", string.Empty);
                    context.AddSource($"{newName}_{index++}.g.cs", SourceText.From(text, Encoding.UTF8));
                }
            }
        }

        private static IEnumerable<ClassDeclarationSyntax> GetAllPartials(Compilation compilation, string className)
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                var root = tree.GetRoot();
                foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    if (cls.Identifier.ValueText == className)
                    {
                        yield return cls;
                    }
                }
            }
        }

        private sealed class RenameRewriter : CSharpSyntaxRewriter
        {
            private readonly string _oldName;
            private readonly string _newName;

            public RenameRewriter(string oldName, string newName)
            {
                _oldName = oldName;
                _newName = newName;
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                if (token.ValueText == _oldName)
                {
                    return SyntaxFactory.Identifier(token.LeadingTrivia, _newName, token.TrailingTrivia);
                }

                return base.VisitToken(token);
            }
        }

        private sealed class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> Candidates { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDecl && classDecl.AttributeLists.Count > 0)
                {
                    Candidates.Add(classDecl);
                }
            }
        }
    }
}
