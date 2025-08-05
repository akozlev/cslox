namespace CraftingInterpreters.Lox;

class Env
{
    private readonly Env _enclosing;
    private readonly Dictionary<string, object> _values = new();

    internal Env()
    {
        _enclosing = null;
    }

    internal Env(Env enclosing)
    {
        _enclosing = enclosing;
    }

    internal void Define(string name, object value)
    {
        _values[name] = value;
    }

    private Env Ancestor(int distance)
    {
        var environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment._enclosing;
        }
        return environment;
    }

    internal object Get(Token name)
    {
        object value;
        if (_values.TryGetValue(name.Lexeme, out value))
        {
            return value;
        }

        if (_enclosing is not null)
        {
            return _enclosing.Get(name);
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

        if (_enclosing is not null)
        {
            _enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Assignment to undefined variable {name.Lexeme}.");
    }

    internal object GetAt(int distance, string name)
    {
        return Ancestor(distance)._values[name];
    }

    internal void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance)._values.Add(name.Lexeme, value);
    }
}
