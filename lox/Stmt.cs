namespace CraftingInterpreters.Lox;

internal abstract class Stmt
{
    internal interface IExprVisitor<R>
    {
        R Visit(ExpressionStatment stmt);
        R Visit(Print stmt);
        R Visit(Var stmt);
    }

    internal abstract R Accept<R>(IExprVisitor<R> visitor);

    internal class ExpressionStatment : Stmt
    {
        public Expr Expression { get; }

        internal ExpressionStatment(Expr expression)
        {
            Expression = expression;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

    internal class Print : Stmt
    {
        public Expr Expression { get; }

        internal Print(Expr expression)
        {
            Expression = expression;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

    internal class Var : Stmt
    {
        public Token Name { get; }
        public Expr Initializer { get; }

        internal Var(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

}
