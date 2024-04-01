using System.Collections;

namespace Scanner;

public delegate ScanResult<TTokenType, TError> TokenScanner<TTokenType, TError>(Caret caret);

public class Scanner<TTokenType, TError> : IEnumerable<Token<TTokenType, TError>> where TTokenType : Enum
{
    private Caret _caret;
    private readonly IEnumerable<TokenScanner<TTokenType, TError>> _tokenScanners;

    public Scanner(string content, IEnumerable<TokenScanner<TTokenType, TError>> tokenScanners)
    {
        _caret = new Caret(content);
        _tokenScanners = tokenScanners;
    }

    public IEnumerator<Token<TTokenType, TError>> GetEnumerator()
    {
        _caret.Reset();

        for (; !_caret.IsEnd();)
            if (IterOverTokenScanners() is { } iterResult)
            {
                if (iterResult.ErrorToken is { } errorToken) yield return errorToken;
                yield return iterResult.Token;
            }

        if (HandleUnexpectedSymbols() is { } e) yield return e;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private struct IterResult
    {
        public Token<TTokenType, TError>? ErrorToken { get; init; }

        public Token<TTokenType, TError> Token { get; init; }
    }

    private IterResult? IterOverTokenScanners()
    {
        foreach (var tokenScanner in _tokenScanners)
        {
            var tempCaret = TempCaret();

            var token = tokenScanner(tempCaret).ToToken(tempCaret.Span());
            if (token == null) continue;

            var errorToken = HandleUnexpectedSymbols();

            UpdateCaret(tempCaret);
            return new IterResult { ErrorToken = errorToken, Token = token };
        }

        _caret.Eat();
        return null;
    }

    private Token<TTokenType, TError>? HandleUnexpectedSymbols()
    {
        return _caret.Span().IsNotEmpty() ? new Token<TTokenType, TError>.InvalidToken(default, _caret.Span()) : null;
    }

    private Caret TempCaret()
    {
        var tempCaret = _caret.Clone();
        tempCaret.ResetStartPos();

        return tempCaret;
    }

    private void UpdateCaret(Caret caret)
    {
        _caret = caret;
        _caret.ResetStartPos();
    }
}