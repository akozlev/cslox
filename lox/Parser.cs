namespace CraftingInterpreters.Lox;

using System;
using static TokenType;

class Parser
{
    private class ParserError : Exception { }

    private readonly List<Token> _tokens;
    private int current = 0;

    public Parser(ICollection<Token> tokens)
    {
        _tokens = tokens.ToList();
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(CLASS))
                return ClassDeclaration();

            if (Match(FUN))
                return Function("function");

            if (Match(VAR))
                return VarDeclaration();

            return Statement();
        }
        catch
        {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expected class name.");

        Expr.Variable superclass = null;
        if (Match(LESS))
        {
            Consume(IDENTIFIER, "Expected superclass name.");
            superclass = new Expr.Variable(Previous());
        }

        Consume(LEFT_BRACE, "Expect '{' before class body.");

        var methods = new List<Stmt.Function>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }

        Consume(RIGHT_BRACE, "Expect '}' after class body.");

        return new Stmt.Class(name, superclass, methods);
    }

    private Stmt VarDeclaration()
    {
        Token name = Consume(IDENTIFIER, "Expected variable name.");

        var intializer = MatchOrNull(EQUAL, Expression);
        Consume(SEMICOLON, "Expect ';' after variable declaration.");

        return new Stmt.Var(name, intializer);
    }

    private Stmt Statement()
    {
        if (Match(FOR))
            return ForStatement();

        if (Match(IF))
            return IfStatement();

        if (Match(PRINT))
            return PrintStatement();

        if (Match(RETURN))
            return ReturnStatment();

        if (Match(WHILE))
            return WhileStatement();

        if (Match(LEFT_BRACE))
            return new Stmt.Block(Block());

        return ExpressionStatemnt();
    }

    private Stmt ForStatement()
    {
        Consume(LEFT_PAREN, "Exprect '(' after 'if'.");

        Stmt initializer;
        if (Match(SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatemnt();
        }

        Expr condition = null;
        if (!Check(SEMICOLON))
        {
            condition = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expr increment = null;
        if (!Check(RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

        var body = Statement();

        if (increment != null)
        {
            body = new Stmt.Block(new List<Stmt> { body, new Stmt.Expression(increment) });
        }

        if (condition == null)
        {
            condition = new Expr.Literal(true);
        }
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block(new List<Stmt> { initializer, body });
        }

        return body;
    }

    private Stmt WhileStatement()
    {
        Consume(LEFT_PAREN, "Exprect '(' after 'if'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Exprect '(' after 'if'.");

        var body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "Exprect '(' after 'if'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Exprect '(' after 'if'.");

        var consequence = Statement();
        var alternative = MatchOrNull(ELSE, Statement);

        return new Stmt.If(condition, consequence, alternative);
    }

    private IList<Stmt> Block()
    {
        var statments = new List<Stmt>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statments.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expected '}' after block.");
        return statments;
    }

    private Stmt ExpressionStatemnt()
    {
        var value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Expression(value);
    }

    private Stmt.Function Function(string kind)
    {
        Token name = Consume(IDENTIFIER, $"Expect {kind} name.");

        Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");
        List<Token> parameters = new();

        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters");
                }

                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }

        Consume(RIGHT_PAREN, "Expect ')' after parameters.");

        Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");

        var body = Block();

        return new Stmt.Function(name, parameters, body);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt ReturnStatment()
    {
        var keyword = Previous();
        Expr value = null;

        if (!Check(SEMICOLON))
        {
            value = Expression();
        }
        Consume(SEMICOLON, "Expect ';' after return value.");

        return new Stmt.Return(keyword, value);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Or();

        if (Match(EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable { Name: var name })
            {
                return new Expr.Assign(name, value);
            }
            else if (expr is Expr.Get get)
            {
                return new Expr.Set(get.Object, get.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(OR))
        {
            var op = Previous();
            var right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(AND))
        {
            var op = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();

        while (Match(MINUS, PLUS))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(SLASH, STAR))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        return Call();
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = new();

        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Call()
    {
        Expr expr = Primary();

        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if (Match(DOT))
            {
                var name = Consume(IDENTIFIER, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(FALSE))
        {
            return new Expr.Literal(false);
        }
        if (Match(TRUE))
        {
            return new Expr.Literal(true);
        }
        if (Match(NIL))
        {
            return new Expr.Literal(null);
        }

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(SUPER))
        {
            var keyword = Previous();
            Consume(DOT, "Expect '.' after 'super'.");
            Token method = Consume(IDENTIFIER, "Expect superclass method name.");
            return new Expr.Super(keyword, method);
        }

        if (Match(THIS))
        {
            return new Expr.This(Previous());
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(RIGHT_PAREN, "Exprect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            return Advance();
        }

        throw Error(Peek(), message);
    }

    private ParserError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParserError();
    }

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON)
            {
                return;
            }

            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private T MatchOrNull<T>(TokenType token, Func<T> func)
    {
        if (Match(token))
        {
            return func();
        }
        else
        {
            return default(T);
        }
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            current++;
        }
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Previous()
    {
        return _tokens[current - 1];
    }

    private Token Peek()
    {
        return _tokens[current];
    }
}
