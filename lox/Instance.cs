namespace CraftingInterpreters.Lox;

class Instance
{
    private Class _class;

    public Instance(Class @class)
    {
        _class = @class;
    }

    public override string ToString()
    {
        return $"{_class.Name} instance";
    }
}
