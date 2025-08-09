namespace CraftingInterpreters.Lox;

class Function : ICallable
{
    private readonly Stmt.Function _declaration;
    private readonly Env _closure;
    private readonly bool _isInitializer;

    public Function(Stmt.Function declaration, Env closure, bool isInitializer)
    {
        _isInitializer = isInitializer;
        _closure = closure;
        _declaration = declaration;
    }

    public int Arity => _declaration.Parameters.Count;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var env = new Env(_closure);

        for (int i = 0; i < _declaration.Parameters.Count; i++)
        {
            env.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, env);
        }
        catch (Return returnValue)
        {
            if (_isInitializer)
                return _closure.GetAt(0, "this");

            return returnValue.Value;
        }

        if (_isInitializer)
            return _closure.GetAt(0, "this");
        return null;
    }

    public override string ToString()
    {
        return $"<fn {_declaration.Name.Lexeme}";
    }

    internal Function Bind(Instance instance)
    {
        var environment = new Env(_closure);
        environment.Define("this", instance);
        return new Function(_declaration, environment, _isInitializer);
    }
}
