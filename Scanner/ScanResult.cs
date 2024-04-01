namespace Scanner;

public abstract class ScanResult<TTokenType, TError>
{
    private ScanResult()
    {
    }

    internal abstract Token<TTokenType, TError>? ToToken(Span span);

    public static ScanResult<TTokenType, TError> Token(TTokenType type)
    {
        return new TokenVariant { Type = type };
    }

    public static ScanResult<TTokenType, TError> Nothing()
    {
        return new NothingVariant();
    }

    public static ScanResult<TTokenType, TError> Error(TError error)
    {
        return new ErrorVariant { Error = error };
    }

    private class TokenVariant : ScanResult<TTokenType, TError>
    {
        public TTokenType Type { get; init; }

        internal override Token<TTokenType, TError>? ToToken(Span span)
        {
            return new Token<TTokenType, TError>.ValidToken(Type, span);
        }
    }

    private class NothingVariant : ScanResult<TTokenType, TError>
    {
        internal override Token<TTokenType, TError>? ToToken(Span span)
        {
            return null;
        }
    }

    private class ErrorVariant : ScanResult<TTokenType, TError>
    {
        public TError Error { get; init; }

        internal override Token<TTokenType, TError>? ToToken(Span span)
        {
            return new Token<TTokenType, TError>.InvalidToken(Error, span);
        }
    }
}