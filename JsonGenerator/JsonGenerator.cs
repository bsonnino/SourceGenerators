using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

[Generator]
public class JsonGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a callback for syntax receiver creation
        context.RegisterForSyntaxNotifications(() => new SerializationSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Get the compilation object that represents all user code being compiled
        var compilation = context.Compilation;


        // Get the syntax receiver that was created by our callback
        var receiver = context.SyntaxReceiver as SerializationSyntaxReceiver;
        if (receiver == null)
        {
            return;
        }

        // Loop through all the syntax trees in the compilation
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            // Get the semantic model for the syntax tree
            var model = compilation.GetSemanticModel(syntaxTree);

            // Loop through all the class declarations
            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                try
                {
                    // Get the symbol for the class declaration
                    var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                    if (classSymbol != null)
                    {
                        // Generate the extension method for the ToJson method
                        var source = GenerateSource(classSymbol);

                        // Add a new source file to the compilation with a unique hint name
                        context.AddSource($"{classSymbol.Name}.ToJson.cs", SourceText.From(source, Encoding.UTF8));
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex);
                }
            }
        }
    }

    private string GenerateSource(INamedTypeSymbol classSymbol)
    {
        // Get the name of the class
        var className = classSymbol.Name;

        // Get the properties of the class
        var properties = classSymbol.GetMembers().OfType<IPropertySymbol>();
        if (!properties.Any())
        {
            return "";
        }

        // Generate code for the ToJson method using StringBuilder
        var builder = new StringBuilder();
        builder.AppendLine($@"
using System.Text.Json;

public static class {className}Extensions
{{
    public static string ToJson(this {className} {className.ToLower()})
    {{");

        // Append code to create a new JSON object as a string using string interpolation and escaping
        builder.Append(@"        return $""{{");
        foreach (var property in properties)
        {
            builder.Append($@"\""{property.Name}\"":\""{{{className.ToLower()}.{property.Name}}}\"",");
        }
        builder.Remove(builder.Length - 1, 1); // Remove trailing comma
        builder.Append(@"}}"";");
        builder.Append(@$"
    }}
        
    public static {className} FromJson(string json)
    {{");

        // Append code to parse the JSON string as a JSON object using JsonDocument.Parse
        builder.AppendLine($@"
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;");

        // Append code to create a new instance of the class using its default constructor
        builder.AppendLine($@"
        var {className.ToLower()} = new {className}();");

        // Append code to assign each property value from the JSON object using JsonElement.GetProperty and TryGet methods
        foreach (var property in properties)
        {
            builder.AppendLine($@"
        if (root.TryGetProperty(""{property.Name}"", out var {property.Name}Element))
        {{
            {className.ToLower()}.{property.Name} = {GetConversionCode(property.Type, $"{property.Name}Element")};
        }}");
        }

        // Append code to return the created object as a result
        builder.AppendLine($@"
        return {className.ToLower()};
    }}
}}");

        // Return the generated source code as a string
        return builder.ToString();
    }

    private string GetConversionCode(ITypeSymbol type, string value)
    {
        // Generate code to convert a JsonElement value to a given type using switch expression and JsonElement.Get methods
        return type switch
        {
            INamedTypeSymbol namedType when namedType.SpecialType == SpecialType.System_String => $"{value}.GetString()",
            INamedTypeSymbol namedType when namedType.SpecialType == SpecialType.System_Int32 => $"{value}.GetInt32()",
            INamedTypeSymbol namedType when namedType.SpecialType == SpecialType.System_Double => $"{value}.GetDouble()",
            INamedTypeSymbol namedType when namedType.SpecialType == SpecialType.System_Boolean => $"{value}.GetBoolean()",
            INamedTypeSymbol namedType when namedType.SpecialType == SpecialType.System_DateTime => $"{value}.GetDateTime()",
            _ => throw new NotSupportedException($"Unsupported type: {type}")
        };
    }

}

// A custom syntax receiver that collects all the classes with the [Serializable] attribute 

public class SerializationSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        Console.WriteLine(syntaxNode);
        // Check if the syntax node is a class declaration with the [Serializable] attribute
        if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.AttributeLists.Count > 0 &&
            classDeclaration.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == "Serializable")))
        {
            // Add it to the candidate list
            CandidateClasses.Add(classDeclaration);
        }
    }
}