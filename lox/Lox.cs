namespace CraftingInterpreters.Lox;

using System.Text;

class Lox
{
    private static readonly Interpreter _interpreter = new();
    private static bool _hadError;
    private static bool _hadRuntimeError;

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
        Run(new String(Encoding.UTF8.GetChars(bytes)));
        if (_hadError)
        {
            Environment.Exit(65);
        }
        if (_hadRuntimeError)
        {
            Environment.Exit(70);
        }
    }

    private static void RunPrompt()
    {
        string line;
        Console.Write("> ");
        while ((line = Console.ReadLine()) is not null)
        {
            Run(line);
            _hadError = false;
            Console.Write("\n> ");
        }
        Environment.Exit(64);
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var statements = parser.Parse();
        if (_hadError)
            return;
        _interpreter.Interpret(statements);
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        _hadError = true;
    }

    internal static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    internal static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
        _hadRuntimeError = true;
    }
}
