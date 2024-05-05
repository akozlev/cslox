namespace CraftingInterpreters.Lox;

internal abstract class Stmt
{
    internal interface IExprVisitor<R>
    {
        R Visit(Block stmt);
        R Visit(ExpressionStatment stmt);
        R Visit(If stmt);
        R Visit(Print stmt);
        R Visit(Var stmt);
        R Visit(While stmt);
    }

    internal abstract R Accept<R>(IExprVisitor<R> visitor);

    internal class Block : Stmt
    {
        public IList<Stmt> Statements { get; }

        internal Block(IList<Stmt> statements)
        {
            Statements = statements;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

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

    internal class If : Stmt
    {
        public Expr Condition { get; }
        public Stmt Consequent { get; }
        public Stmt Alternative { get; }

        internal If(Expr condition, Stmt consequent, Stmt alternative)
        {
            Condition = condition;
            Consequent = consequent;
            Alternative = alternative;
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

    internal class While : Stmt
    {
        public Expr Condition { get; }
        public Stmt Body { get; }

        internal While(Expr condition, Stmt body)
        {
            Condition = condition;
            Body = body;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

}
