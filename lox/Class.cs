namespace CraftingInterpreters.Lox;

class Class : ICallable
{
    public readonly string Name;
    public readonly Class Superclass;
    private readonly Dictionary<string, Function> _methods;

    public int Arity
    {
        get
        {
            if (FindMethod("init") is { } initializer)
            {
                return initializer.Arity;
            }

            return 0;
        }
    }

    public Class(string name, Class superclass, Dictionary<string, Function> methods)
    {
        Superclass = superclass;
        Name = name;
        _methods = methods;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new Instance(this);

        if (FindMethod("init") is { } initializer)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }

        return instance;
    }

    public override string ToString()
    {
        return Name;
    }

    internal Function FindMethod(string name)
    {
        if (_methods.TryGetValue(name, out var method))
        {
            return method;
        }

        if (Superclass is not null)
        {
            return Superclass.FindMethod(name);
        }

        return null;
    }
}
