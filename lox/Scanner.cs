namespace CraftingInterpreters.Lox;

using static TokenType;

class Scanner
{
    private readonly string source;
    private readonly ICollection<Token> tokens = new List<Token>();

    private int start = 0;
    private int current = 0;
    private int line = 1;

    private bool IsAtEnd => current >= source.Length;

    private static readonly Dictionary<string, TokenType> Keywords =
        new()
        {
            { "and", AND },
            { "class", CLASS },
            { "else", ELSE },
            { "false", FALSE },
            { "for", FOR },
            { "fun", FUN },
            { "if", IF },
            { "nil", NIL },
            { "or", OR },
            { "print", PRINT },
            { "return", RETURN },
            { "super", SUPER },
            { "this", THIS },
            { "true", TRUE },
            { "var", VAR },
            { "while", WHILE },
        };

    internal Scanner(string source)
    {
        this.source = source;
    }

    internal ICollection<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(EOF, "", null, line));
        return tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(':
                AddToken(LEFT_PAREN);
                break;
            case ')':
                AddToken(RIGHT_PAREN);
                break;

            case '{':
                AddToken(LEFT_BRACE);
                break;
            case '}':
                AddToken(RIGHT_BRACE);
                break;

            case ',':
                AddToken(COMMA);
                break;
            case '.':
                AddToken(DOT);
                break;

            case '-':
                AddToken(MINUS);
                break;
            case '+':
                AddToken(PLUS);
                break;

            case ';':
                AddToken(SEMICOLON);
                break;
            case '*':
                AddToken(STAR);
                break;

            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;

            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;

            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;

            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;

            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd)
                        Advance();
                }
                else
                {
                    AddToken(SLASH);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                break;

            case '\n':
                line++;
                break;
            case '"':
                String();
                break;

            default:
                // In comparison with Java, C#'s char.IsDigit only includes the characters 0 through 9.
                if (char.IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    Lox.Error(line, "Unexpecte character.");
                }
                break;
        }
    }

    private char Advance() => source.ElementAt(current++);

    private void AddToken(TokenType type) => AddToken(type, null);

    private void AddToken(TokenType type, object literal)
    {
        var text = source[start..current];
        tokens.Add(new Token(type, text, literal, line));
    }

    private bool Match(char expected)
    {
        if (IsAtEnd)
            return false;
        if (source.ElementAt(current) != expected)
            return false;

        current++;
        return true;
    }

    private char Peek() => IsAtEnd ? '\0' : source.ElementAt(current);

    private char PeekNext() => current + 1 >= source.Length ? '\0' : source.ElementAt(current + 1);

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
            Advance();

        var text = source[start..current];
        AddToken(Keywords.TryGetValue(text, out var type) ? type : IDENTIFIER);
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n')
            {
                line++;
            }
            Advance();
        }

        if (IsAtEnd)
        {
            Lox.Error(line, "Untreminated string.");
            return;
        }

        Advance();

        var value = source[(start + 1)..(current - 1)];
        AddToken(STRING, value);
    }

    private void Number()
    {
        while (char.IsDigit(Peek()))
        {
            Advance();
        }

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();

            while (char.IsDigit(Peek()))
            {
                Advance();
            }
        }

        AddToken(NUMBER, double.Parse(source[start..current]));
    }

    private bool IsAlpha(char c) => char.IsAsciiLetter(c) || c == '_';

    private bool IsAlphaNumeric(char c) => char.IsAsciiLetterOrDigit(c) || c == '_';
}
