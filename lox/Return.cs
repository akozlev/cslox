namespace CraftingInterpreters.Lox;

class Return : Exception
{
    public object Value { get; }

    public Return(Object value)
    {
        Value = value;
    }
}
