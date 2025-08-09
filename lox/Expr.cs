namespace CraftingInterpreters.Lox;

internal abstract class Expr
{
    internal interface IExprVisitor<R>
    {
        R Visit(Assign expr);
        R Visit(Binary expr);
        R Visit(Call expr);
        R Visit(Get expr);
        R Visit(Grouping expr);
        R Visit(Literal expr);
        R Visit(Logical expr);
        R Visit(Set expr);
        R Visit(Super expr);
        R Visit(This expr);
        R Visit(Unary expr);
        R Visit(Variable expr);
    }

    internal abstract R Accept<R>(IExprVisitor<R> visitor);

    internal class Assign : Expr
    {
        public Token Name { get; }
        public Expr Value { get; }

        internal Assign(Token name, Expr value)
        {
            Name = name;
            Value = value;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

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
            return visitor.Visit(this); 
        }
    }

    internal class Call : Expr
    {
        public Expr Callee { get; }
        public Token Paren { get; }
        public IList<Expr> Arguments { get; }

        internal Call(Expr callee, Token paren, IList<Expr> arguments)
        {
            Callee = callee;
            Paren = paren;
            Arguments = arguments;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

    internal class Get : Expr
    {
        public Expr Object { get; }
        public Token Name { get; }

        internal Get(Expr @object, Token name)
        {
            Object = @object;
            Name = name;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
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
            return visitor.Visit(this); 
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
            return visitor.Visit(this); 
        }
    }

    internal class Logical : Expr
    {
        public Expr Left { get; }
        public Token Op { get; }
        public Expr Right { get; }

        internal Logical(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

    internal class Set : Expr
    {
        public Expr Object { get; }
        public Token Name { get; }
        public Expr Value { get; }

        internal Set(Expr @object, Token name, Expr value)
        {
            Object = @object;
            Name = name;
            Value = value;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

    internal class Super : Expr
    {
        public Token Keyword { get; }
        public Token Method { get; }

        internal Super(Token keyword, Token method)
        {
            Keyword = keyword;
            Method = method;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

    internal class This : Expr
    {
        public Token Keyword { get; }

        internal This(Token keyword)
        {
            Keyword = keyword;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
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
            return visitor.Visit(this); 
        }
    }

    internal class Variable : Expr
    {
        public Token Name { get; }

        internal Variable(Token name)
        {
            Name = name;
        }

        internal override R Accept<R>(IExprVisitor<R> visitor)
        {
            return visitor.Visit(this); 
        }
    }

}
