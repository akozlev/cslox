namespace CraftingInterpreters.Lox;

internal abstract class Expr
{
    internal interface IExprVisitor<R>
    {
        R VisitBinaryExpr(Binary expr);
        R VisitGroupingExpr(Grouping expr);
        R VisitLiteralExpr(Literal expr);
        R VisitUnaryExpr(Unary expr);
    }

    internal abstract R Accept<R>(IExprVisitor<R> visitor);

    internal class Binary : Expr
    {
        public Expr Left { get; }
        public Token Op { get; }
        public Expr Right { get; }

        internal Binary(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.VisitBinaryExpr(this); 
        }
    }

    internal class Grouping : Expr
    {
        public Expr Expression { get; }

        internal Grouping(Expr expression)
        {
            Expression = expression;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.VisitGroupingExpr(this); 
        }
    }

    internal class Literal : Expr
    {
        public Object Value { get; }

        internal Literal(Object value)
        {
            Value = value;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.VisitLiteralExpr(this); 
        }
    }

    internal class Unary : Expr
    {
        public Token Op { get; }
        public Expr Right { get; }

        internal Unary(Token op, Expr right)
        {
            Op = op;
            Right = right;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.VisitUnaryExpr(this); 
        }
    }

}
