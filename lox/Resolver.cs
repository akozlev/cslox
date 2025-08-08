namespace CraftingInterpreters.Lox;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 0)]
public struct Void { }

class Resolver : Expr.IExprVisitor<Void>, Stmt.IExprVisitor<Void>
{
    private readonly Interpreter _interpreter;
    private Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType _currentFunction = FunctionType.None;

    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public void Resolve(IList<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private enum FunctionType
    {
        None,
        Function,
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope()
    {
        scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (scopes.TryPeek(out var scope))
        {
            if (scope.TryAdd(name.Lexeme, false) is false)
            {
                Lox.Error(name, "Already a variable with this name in this scope.");
            }
        }
    }

    private void Define(Token name)
    {
        if (scopes.TryPeek(out var scope))
        {
            scope[name.Lexeme] = true;
        }
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                _interpreter.Resolve(expr, scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void ResolveFunction(Stmt.Function stmt, FunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;

        BeginScope();
        foreach (var param in stmt.Parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(stmt.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    public Void Visit(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return default;
    }

    public Void Visit(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return default;
    }

    public Void Visit(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.Function);
        return default;
    }

    public Void Visit(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Consequent);
        if (stmt.Alternative is { } alternative)
        {
            Resolve(alternative);
        }
        return default;
    }

    public Void Visit(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return default;
    }

    public Void Visit(Stmt.Return stmt)
    {
        if (_currentFunction is FunctionType.None)
        {
            Lox.Error(stmt.Keyword, "Can't return top-level code.");
        }

        if (stmt.Value is { } value)
        {
            Resolve(value);
        }
        return default;
    }

    public Void Visit(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer is not null)
        {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);

        return default;
    }

    public Void Visit(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return default;
    }

    public Void Visit(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return default;
    }

    public Void Visit(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return default;
    }

    public Void Visit(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (var argument in expr.Arguments)
        {
            Resolve(argument);
        }

        return default;
    }

    public Void Visit(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return default;
    }

    public Void Visit(Expr.Literal expr)
    {
        return default;
    }

    public Void Visit(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return default;
    }

    public Void Visit(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return default;
    }

    public Void Visit(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return default;
    }

    public Void Visit(Expr.Variable expr)
    {
        if (
            scopes.TryPeek(out var scope)
            && scope.TryGetValue(expr.Name.Lexeme, out var value)
            && value is false
        )
        {
            Lox.Error(expr.Name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return default;
    }

    public Void Visit(Expr.Get expr)
    {
        Resolve(expr.Object);
        return default;
    }

    public Void Visit(Stmt.Class stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        return default;
    }
}
