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
            runFile(args[0]);
        }
        else
        {
            runPrompt();
        }
    }

    private static void runFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        run(new String(Encoding.Unicode.GetChars(bytes)));
        if (hadError) Environment.Exit(65);
    }

    private static void runPrompt()
    {
        string line;
        Console.Write("> ");
        while ((line = Console.ReadLine()) is not null)
        {
            run(line);
            hadError = false;
            Console.Write("\n> ");
        }
        Environment.Exit(64);
    }

    private static void run(string source)
    {
        var scanner = new Scanner(source);
        foreach (var token in scanner.ScanTokens())
        {
            Console.Write(token);
        }
    }

    internal static void error(int line, string message)
    {
        report(line, "", message);
    }

    private static void report(int line, string where, string message)
    {
        Console.Error.Write($"[line {line}] Error{where}: {message}");
        hadError = true;
    }
}
