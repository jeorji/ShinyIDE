// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using Scanner;

// var scanner = new Scanner<TokenType>("const abcd: &str = \"te\\\"st\"", TokensScanners.TokenScanners);
// var scanner = new Scanner<TokenType>("const ab@!^)c^d: &str = \"te\\\"st", TokensScanners.TokenScanners);
var scanner = new Scanner<TokenType>("const@", TokensScanners.TokenScanners);
var tokens = scanner.Scan();
Console.WriteLine("Hello, World!");

public enum TokenType
{
    ConstKeyword,
    Identifier,
    Colon,
    Ampersand,
    StrKeyword,
    AssignmentOperator,
    StringLiteral,
    OperatorEnd,
    Separator
}

public static class TokensScanners
{
    public static string TokenKind(this TokenType type)
    {
        return type switch
        {
            TokenType.ConstKeyword => "keyword",
            TokenType.Identifier => "identifier",
            TokenType.Colon => "type divider"
        };
    }


    public static readonly IEnumerable<TokenScanner<TokenType>> TokenScanners = new TokenScanner<TokenType>[]
    {
        caret => ScanOneSymbolToken(caret, ':', TokenType.Colon),
        caret => ScanOneSymbolToken(caret, '&', TokenType.Ampersand),
        caret => ScanOneSymbolToken(caret, '=', TokenType.AssignmentOperator),
        caret => ScanOneSymbolToken(caret, ';', TokenType.OperatorEnd),
        ScanKeywords,
        ScanIdentifier,
        ScanStringLiteralToken,
        ScanSeparatorToken
    };

    private static ScanResult<TokenType> ScanKeywords(Caret caret)
    {
        if (!caret.TryEatWhile(char.IsLetterOrDigit)) return ScanResult<TokenType>.Nothing();

        return caret.Slice() switch
        {
            "const" => ScanResult<TokenType>.Token(TokenType.ConstKeyword),
            "str" => ScanResult<TokenType>.Token(TokenType.StrKeyword),
            _ => ScanResult<TokenType>.Nothing()
        };
    }

    private static ScanResult<TokenType> ScanIdentifier(Caret caret)
    {
        if (!caret.TryEatWhile(char.IsLetterOrDigit)) return ScanResult<TokenType>.Nothing();

        var value = caret.Slice();

        return value.Length > 0 && char.IsLetter(value[0])
            ? ScanResult<TokenType>.Token(TokenType.Identifier)
            : ScanResult<TokenType>.Nothing();
    }

    private static ScanResult<TokenType> ScanOneSymbolToken(Caret caret, char symbol, TokenType type)
    {
        return caret.TryEat(symbol)
            ? ScanResult<TokenType>.Token(type)
            : ScanResult<TokenType>.Nothing();
    }

    private static ScanResult<TokenType> ScanStringLiteralToken(Caret caret)
    {
        if (!caret.TryEat('"')) return ScanResult<TokenType>.Nothing();

        caret.TryEatWhile(IsStringLiteralEnd(caret));

        return caret.TryEat('"')
            ? ScanResult<TokenType>.Token(TokenType.StringLiteral)
            : ScanResult<TokenType>.Error("unterminated string");
    }

    private static Func<char, bool> IsStringLiteralEnd(Caret caret)
    {
        return symbol =>
        {
            caret.TryEat('\\', '"');
            return symbol != '"';
        };
    }

    private static ScanResult<TokenType> ScanSeparatorToken(Caret caret)
    {
        return caret.TryEatWhile(char.IsSeparator)
            ? ScanResult<TokenType>.Token(TokenType.Separator)
            : ScanResult<TokenType>.Nothing();
    }
}