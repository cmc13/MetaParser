using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;

namespace MetaParser.WPF.ViewModelGenerator
{
    [Generator]
    public class ViewModelGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Debug.WriteLine("Executing source generator");
            foreach (var tree in context.Compilation.SyntaxTrees)
            {
                continue;
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }
    }
}
