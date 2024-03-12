namespace CraftingInterpreters.Lox;

using System.Text;

class Lox
{
    static bool hadError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: cslox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunAstPrinter()
    {
        Expr expression = new Expr.Binary(
            new Expr.Unary(new Token(TokenType.MINUS, "-", null, 1), new Expr.Literal(123)),
            new Token(TokenType.STAR, "*", null, 1),
            new Expr.Grouping(new Expr.Literal(45.67))
        );
        Console.WriteLine(new AstPrinter().Print(expression));
    }

    private static void RunFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Run(new String(Encoding.Unicode.GetChars(bytes)));
        if (hadError)
        {
            Environment.Exit(65);
        }
    }

    private static void RunPrompt()
    {
        string line;
        Console.Write("> ");
        while ((line = Console.ReadLine()) is not null)
        {
            Run(line);
            hadError = false;
            Console.Write("\n> ");
        }
        Environment.Exit(64);
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        foreach (var token in scanner.ScanTokens())
        {
            Console.Write(token);
        }
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.Write($"[line {line}] Error{where}: {message}");
        hadError = true;
    }
}
