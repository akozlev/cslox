namespace CraftingInterpreters.Lox;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 0)]
public struct Void { }

class Resolver : Expr.IExprVisitor<Void>, Stmt.IExprVisitor<Void>
{
    private readonly Interpreter _interpreter;
    private Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;
    private ClassType _currentClass = ClassType.None;

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
        Initializer,
        Method,
    }

    private enum ClassType
    {
        None,
        Class,
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
        _scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (_scopes.TryPeek(out var scope))
        {
            if (scope.TryAdd(name.Lexeme, false) is false)
            {
                Lox.Error(name, "Already a variable with this name in this scope.");
            }
        }
    }

    private void Define(Token name)
    {
        if (_scopes.TryPeek(out var scope))
        {
            scope[name.Lexeme] = true;
        }
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        foreach (var (index, scope) in _scopes.Index())
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                _interpreter.Resolve(expr, index);
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
            if (_currentFunction is FunctionType.Initializer)
            {
                Lox.Error(stmt.Keyword, "Can't return a value from an initializer.");
            }
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

    public Void Visit(Expr.This expr)
    {
        if (_currentClass is ClassType.None)
        {
            Lox.Error(expr.Keyword, "Can't use 'this' outside of a class.");
            return default;
        }

        ResolveLocal(expr, expr.Keyword);
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
            _scopes.TryPeek(out var scope)
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
        var enclosingClass = _currentClass;
        _currentClass = ClassType.Class;

        Declare(stmt.Name);
        Define(stmt.Name);

        BeginScope();
        _scopes.Peek()["this"] = true;

        foreach (var method in stmt.Methods)
        {
            var declaration = method.Name.Lexeme.Equals("init")
                ? FunctionType.Initializer
                : FunctionType.Method;
            ResolveFunction(method, declaration);
        }

        EndScope();

        _currentClass = enclosingClass;
        return default;
    }
}
