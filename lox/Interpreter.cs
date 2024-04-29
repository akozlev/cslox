namespace CraftingInterpreters.Lox;

using static TokenType;

class Interpreter : Expr.IExprVisitor<object>
{
    public void Interpret(Expr expression)
    {
        try
        {
            var value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeError error)
        {
            Lox.RuntimeError(error);
        }
    }

    public object VisitBinaryExpr(Expr.Binary expr)
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

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
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
}
