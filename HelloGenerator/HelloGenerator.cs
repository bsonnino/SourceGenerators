using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HelloWorldGenerator;

[Generator]
public class HelloGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Create the source to inject
        var sourceBuilder = new StringBuilder(@"
        using System;
namespace HelloWorldGenerated
{
    public static class HelloWorld
    {
        public static void SayHello()
        {
            Console.WriteLine(""Hello from generated code!"");
        }
    }
}");
        // Add the source file to the compilation
        context.AddSource("HelloWorld", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}
