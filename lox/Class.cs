namespace CraftingInterpreters.Lox;

class Class : ICallable
{
    public readonly string Name;

    public int Arity => 0;

    public Class(string name)
    {
        Name = name;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new Instance(this);
        return instance;
    }

    public override string ToString()
    {
        return Name;
    }
}
