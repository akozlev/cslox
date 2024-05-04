namespace CraftingInterpreters.Lox;

using System;
using System.Collections.Generic;
using static TokenType;

class Interpreter : Expr.IExprVisitor<object>, Stmt.IExprVisitor<object>
{
    private Env _environment = new();

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

    public object Visit(Stmt.ExpressionStatment stmt)
    {
        Evaluate(stmt.Expression);
        return null;
    }

    public object Visit(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expression);
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

    private void ExecuteBlock(IList<Stmt> statements, Env environment)
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
}
