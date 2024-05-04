namespace CraftingInterpreters.Lox;

class Env
{
    public readonly Env Enclosing;
    private readonly Dictionary<string, object> _values = new();

    Env()
    {
        Enclosing = null;
    }

    internal void Define(string name, object value)
    {
        _values[name] = value;
    }

    internal object Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        throw new RuntimeError(name, $"Undefined variable {name.Lexeme}.");
    }

    internal void Assign(Token name, object value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        throw new RuntimeError(name, $"Assignment to undefined variable {name.Lexeme}.");
    }
}
