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

    public Expr Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParserError error)
        {
            return null;
        }
    }

    private Expr Expression()
    {
        return Equality();
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL))
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

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(FALSE))
            return new Expr.Literal(false);
        if (Match(TRUE))
            return new Expr.Literal(true);
        if (Match(NIL))
            return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
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
            return Advance();

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
                return;

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

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
            return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd())
            current++;
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
