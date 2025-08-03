namespace CraftingInterpreters.Lox;

using System.Collections.Generic;

interface ICallable
{
    int Arity {
        get;
    }
    object Call(Interpreter interpreter, List<object> arguments); 
}
