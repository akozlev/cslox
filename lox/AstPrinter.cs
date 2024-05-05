using System.Text;

namespace CraftingInterpreters.Lox;

class AstPrinter : Expr.IExprVisitor<String>
{
    public String Print(Expr expr)
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
        throw new NotImplementedException();
    }

    public string Visit(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public string Visit(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }

    private String Parenthesize(String name, params Expr[] exprs)
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
