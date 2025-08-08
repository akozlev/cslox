namespace CraftingInterpreters.Lox;

class Instance
{
    private Class _class;
    private readonly Dictionary<string, object> _fields = new();

    public Instance(Class @class)
    {
        _class = @class;
    }

    public override string ToString()
    {
        return $"{_class.Name} instance";
    }

    internal object Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out var field))
        {
            return field;
        }

        throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'");
    }
}
