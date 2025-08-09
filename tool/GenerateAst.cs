namespace CraftingInterpreters.Tool;

using System.Collections.Generic;

public class GenerateAst
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        string outputDir = args[0];
        DefineAst(
            outputDir,
            "Expr",
            new[]
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token op, Expr right",
                "Call     : Expr callee, Token paren, IList<Expr> arguments",
                "Get      : Expr object, Token name",
                "Grouping : Expr expression",
                "Literal  : Object value",
                "Logical  : Expr left, Token op, Expr right",
                "Set      : Expr object, Token name, Expr value",
                "This     : Token keyword",
                "Unary    : Token op, Expr right",
                "Variable : Token name",
            }
        );

        DefineAst(
            outputDir,
            "Stmt",
            new[]
            {
                "Block      : IList<Stmt> statements",
                "Class      : Token name, IList<Stmt.Function> methods",
                "Expression : Expr expr",
                "Function   : Token name, IList<Token> parameters, IList<Stmt> body",
                "If         : Expr condition, Stmt consequent, Stmt alternative",
                "Print      : Expr expr",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr condition, Stmt body",
            }
        );
    }

    private static void DefineAst(string outputDir, string baseName, IEnumerable<string> types)
    {
        string path = Path.Combine(outputDir, $"{baseName}.cs");
        using StreamWriter output = new StreamWriter(path);

        output.WriteLine("namespace CraftingInterpreters.Lox;");
        output.WriteLine();
        output.WriteLine($"internal abstract class {baseName}");
        output.WriteLine("{");

        DefineVisitor(output, baseName, types);

        output.WriteLine("    internal abstract R Accept<R>(IExprVisitor<R> visitor);");
        output.WriteLine();

        // The AST classes.
        foreach (var type in types)
        {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();
            DefineType(output, baseName, className, fields);
        }

        output.WriteLine("}");
        output.Close();
    }

    private static void DefineVisitor(
        StreamWriter output,
        string baseName,
        IEnumerable<string> types
    )
    {
        output.WriteLine("    internal interface IExprVisitor<R>");
        output.WriteLine("    {");

        foreach (var type in types)
        {
            var typeName = type.Split(":")[0].Trim();
            output.WriteLine($"        R Visit({typeName} {baseName.Safe()});");
        }

        output.WriteLine("    }");
        output.WriteLine();
    }

    private static void DefineType(
        StreamWriter output,
        string baseName,
        string className,
        string fieldList
    )
    {
        var fields = fieldList.Split(", ");

        output.WriteLine($"    internal class {className} : {baseName}");
        output.WriteLine("    {");

        // Properties
        foreach (var field in fields)
        {
            var nameStart = field.IndexOf(' ') + 1;
            output.WriteLine($$"""        public {{field.CharAtToUpper(nameStart)}} { get; }""");
        }
        output.WriteLine();

        var safeFields = new List<string>();
        foreach (var field in fields)
        {
            var type = field.Split(" ")[0];
            var name = field.Split(" ")[1];
            safeFields.Add($"{type} {name.Safe()}");
        }
        // Constructor
        output.WriteLine($"        internal {className}({string.Join(", ", safeFields)})");
        output.WriteLine("        {");
        foreach (var field in fields)
        {
            var name = field.Split(" ")[1];
            output.WriteLine($"            {name.CharAtToUpper()} = {name.Safe()};");
        }
        output.WriteLine("        }");
        output.WriteLine();

        output.WriteLine("        internal override R Accept<R>(IExprVisitor<R> visitor)");
        output.WriteLine("        {");
        output.WriteLine($"            return visitor.Visit(this); ");
        output.WriteLine("        }");

        output.WriteLine("    }");
        output.WriteLine();
    }
}

static class HelperExtensions
{
    public static string CharAtToUpper(this string input, int index = 0)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var chars = input.ToCharArray();
        chars[index] = char.ToUpperInvariant(chars[index]);

        return new string(chars);
    }

    private static HashSet<string> keyword = new(
        new string[]
        {
            "class", //
            "object",
        }
    );

    public static string Safe(this string value)
    {
        var lowered = value.ToLower();
        if (keyword.Contains(lowered))
        {
            return $"@{lowered}";
        }
        return lowered;
    }
}
