using Microsoft.CodeAnalysis;
using System;

namespace SourceCodeGenerator.Net
{
	[Generator]
	public class RecyclableCollectionsGenerator : ISourceGenerator
	{
		public void Execute(GeneratorExecutionContext context)
		{
			context.AddSource($".g.cs");
		}

		public void Initialize(GeneratorInitializationContext context)
		{
			// Nothing to initialize at this point yet
		}
	}
}
