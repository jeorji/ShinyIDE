namespace Scanner;

public class Token<TTokenType, TError>
{
    public Span Span { get; }

    private Token(Span span)
    {
        Span = span;
    }

    public class ValidToken : Token<TTokenType, TError>
    {
        public TTokenType Type { get; }

        public ValidToken(TTokenType type, Span span) : base(span)
        {
            Type = type;
        }
    }

    public class InvalidToken : Token<TTokenType, TError>
    {
        public TError Error { get; }

        public InvalidToken(TError error, Span span) : base(span)
        {
            Error = error;
        }
    }
}