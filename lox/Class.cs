namespace CraftingInterpreters.Lox;

class Class
{
    public readonly string Name;

    public Class(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
