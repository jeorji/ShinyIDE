namespace Compiler.Parser;

public abstract class Lexeme
{
    public Span Span { get; private init; }

    private Lexeme()
    {
    }

    public sealed class Valid : Lexeme
    {
        public LexemeType Type { get; }

        public Valid(Span span, LexemeType type)
        {
            Span = span;
            Type = type;
        }
    }

    public sealed class Invalid : Lexeme
    {
        public Invalid(Span span)
        {
            Span = span;
        }
    }
}
public static class LexemeExtensions
{
    public static Lexeme IntoLexeme(this LexemeType type, Span span)
    {
        return new Lexeme.Valid(span, type);
    }

    public static Lexeme IntoLexeme(this InvalidLexemeType error, Span span)
    {
        return new Lexeme.Invalid(span);
    }
}