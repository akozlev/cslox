namespace CraftingInterpreters.Lox;

using System;
using System.Collections.Generic;
using static TokenType;

class Interpreter : Expr.IExprVisitor<object>, Stmt.IExprVisitor<object>
{
    private readonly Env _globals = new();
    private Env _environment;

    public Env Globals
    {
        get => _globals;
    }

    public Interpreter()
    {
        _environment = _globals;

        _globals.Define("clock", new Clock());
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError error)
        {
            Lox.RuntimeError(error);
        }
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }

    public object Visit(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Op.Type)
        {
            case GREATER:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left <= (double)right;
            case MINUS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left - (double)right;
            case PLUS:
                if (left is double leftD && right is double rightD)
                {
                    return leftD + rightD;
                }
                if (left is string leftStr && right is string rightStr)
                {
                    return leftStr + rightStr;
                }
                throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
            case SLASH:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left * (double)right;
            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
        }

        // Unreachable
        return null;
    }

    public object Visit(Expr.Call expr)
    {
        object callee = Evaluate(expr.Callee);

        var arguments = new List<object>();

        foreach (var arg in expr.Arguments)
        {
            arguments.Add(Evaluate(arg));
        }

        if (callee is not ICallable)
        {
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        }

        var function = (ICallable)callee;

        if (arguments.Count != function.Arity)
        {
            throw new RuntimeError(
                expr.Paren,
                $"Expected {function.Arity} arguments but got {arguments.Count}."
            );
        }

        return function.Call(this, arguments);
    }

    private bool IsEqual(object a, object b)
    {
        if (a is null && b is null)
            return true;
        if (a is null)
            return false;
        return a.Equals(b);
    }

    private string Stringify(object obj)
    {
        if (obj is null)
            return "nil";

        if (obj is double d)
        {
            string text = obj.ToString();
            if (text.EndsWith(".0"))
            {
                text.Substring(0, text.Length - 2);
            }
            return text;
        }

        return obj.ToString();
    }

    public object Visit(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object Visit(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object Visit(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Op.Type)
        {
            case BANG:
                return !IsTruthy(right);
            case MINUS:
                CheckNumberOperand(expr.Op, right);
                return -(double)right;
        }

        // Unreachable
        return null;
    }

    public object Visit(Expr.Variable expr)
    {
        return _environment.Get(expr.Name);
    }

    public object Visit(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        _environment.Assign(expr.Name, value);
        return value;
    }

    private void CheckNumberOperands(Token op, object left, object right)
    {
        if (left is double && right is double)
        {
            return;
        }
        throw new RuntimeError(op, "Operands must be a number");
    }

    private void CheckNumberOperand(Token op, object operand)
    {
        if (operand is double)
        {
            return;
        }
        throw new RuntimeError(op, "Operand must be a number");
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private bool IsTruthy(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is bool)
        {
            return (bool)obj;
        }

        return true;
    }

    public object Visit(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object Visit(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object Visit(Stmt.Var stmt)
    {
        object value = null;
        if (stmt.Initializer is not null)
        {
            value = Evaluate(stmt.Initializer);
        }

        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object Visit(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Env(_environment));
        return null;
    }

    public void ExecuteBlock(IList<Stmt> statements, Env environment)
    {
        var previous = _environment;
        try
        {
            _environment = environment;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    public object Visit(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Consequent);
        }
        else if (stmt.Alternative != null)
        {
            Execute(stmt.Alternative);
        }

        return null;
    }

    public object Visit(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Op.Type == TokenType.OR)
        {
            if (IsTruthy(left))
                return left;
        }
        else
        {
            if (!IsTruthy(left))
                return left;
        }

        return Evaluate(expr.Right);
    }

    public object Visit(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        return null;
    }

    public object Visit(Stmt.Function stmt)
    {
        var func = new Function(stmt);
        _environment.Define(stmt.Name.Lexeme, func);
        return null;
    }

    public object Visit(Stmt.Return stmt)
    {
        object value = null;
        if (stmt.Value is not null)
            value = Evaluate(stmt.Value);

        throw new Return(value);
    }

    private class Clock : ICallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return (double)DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
