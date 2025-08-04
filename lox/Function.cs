namespace CraftingInterpreters.Lox;

class Function : ICallable
{
    private readonly Stmt.Function _declaration;

    public Function(Stmt.Function declaration)
    {
        _declaration = declaration;
    }

    public int Arity => _declaration.Parameters.Count;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var env = new Env(interpreter.Globals);

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
            return returnValue.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {_declaration.Name.Lexeme}";
    }
}
