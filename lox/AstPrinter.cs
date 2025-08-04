using System.Text;

namespace CraftingInterpreters.Lox;

class AstPrinter : Expr.IExprVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string Visit(Expr.Binary expr)
    {
        return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
    }

    public string Visit(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string Visit(Expr.Literal expr)
    {
        if (expr.Value is null)
            return "nil";
        return expr.Value.ToString();
    }

    public string Visit(Expr.Unary expr)
    {
        return Parenthesize(expr.Op.Lexeme, expr.Right);
    }

    public string Visit(Expr.Variable expr)
    {
        return expr.ToString();
    }

    public string Visit(Expr.Assign expr)
    {
        return Parenthesize($"assign {expr.Name}", expr.Value);
    }

    public string Visit(Expr.Logical expr)
    {
        return Parenthesize(expr.Op.ToString(), expr.Left, expr.Right);
    }

    public string Visit(Expr.Call expr)
    {
        return Parenthesize(expr.Callee.ToString(), expr.Arguments.ToArray());
    }

    private string Parenthesize(String name, params Expr[] exprs)
    {
        var sb = new StringBuilder();

        sb.Append("(").Append(name);
        foreach (var expr in exprs)
        {
            sb.Append(" ");
            sb.Append(expr.Accept(this));
        }
        sb.Append(")");

        return sb.ToString();
    }
}
