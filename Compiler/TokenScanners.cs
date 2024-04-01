using System;
using System.Collections.Generic;
using Scanner;

namespace Compiler;

using ScanResult = ScanResult<TokenType, TokenError>;
using TokenScanner = TokenScanner<TokenType, TokenError>;

public enum TokenType
{
    ConstKeyword = 1,
    StrKeyword = 2,
    Identifier = 3,
    StringLiteral = 4,
    Colon = 5,
    Ampersand = 6,
    AssignmentOperator = 7,
    OperatorEnd = 8,
    Separator = 9
}

public enum TokenError
{
    UnexpectedSymbol = 0,
    UnterminatedString,
    IdentifierCanOnlyStartWithANumber
}

public static class TokensScanners
{
    public static readonly IEnumerable<TokenScanner> TokenScanners =
        new TokenScanner[]
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

    private static ScanResult ScanKeywords(Caret caret)
    {
        if (!caret.TryEatWhile(char.IsLetterOrDigit)) return ScanResult.Nothing();

        return caret.Slice() switch
        {
            "const" => ScanResult.Token(TokenType.ConstKeyword),
            "str" => ScanResult.Token(TokenType.StrKeyword),
            _ => ScanResult.Nothing()
        };
    }

    private static ScanResult ScanIdentifier(Caret caret)
    {
        if (!caret.TryEat(s => char.IsLetter(s) || s == '_')) return ScanResult.Nothing();

        caret.TryEatWhile(s => char.IsLetterOrDigit(s) || s == '_');
        // if (!caret.TryEatWhile(s => char.IsLetterOrDigit(s) || s == '_'))
        //     return ScanResult.Error();

        return ScanResult.Token(TokenType.Identifier);
        // var value = caret.Slice();
        //
        // return value.Length > 0 && char.IsLetter(value[0])
        //     ? ScanResult.Token(TokenType.Identifier)
        //     : ScanResult.Error(TokenError.IdentifierCanOnlyStartWithANumber);
    }

    private static ScanResult ScanOneSymbolToken(Caret caret, char symbol, TokenType type)
    {
        return caret.TryEat(symbol)
            ? ScanResult.Token(type)
            : ScanResult.Nothing();
    }

    private static ScanResult ScanStringLiteralToken(Caret caret)
    {
        if (!caret.TryEat('"')) return ScanResult.Nothing();

        caret.TryEatWhile(IsStringLiteralEnd(caret));

        return caret.TryEat('"')
            ? ScanResult.Token(TokenType.StringLiteral)
            : ScanResult.Error(TokenError.UnterminatedString);
    }

    private static Func<char, bool> IsStringLiteralEnd(Caret caret)
    {
        return symbol =>
        {
            caret.TryEat('\\', '"');
            return symbol != '"';
        };
    }

    private static ScanResult ScanSeparatorToken(Caret caret)
    {
        return caret.TryEatWhile((sym, nextSym) => char.IsSeparator(sym) || $"{sym}" == Environment.NewLine ||
                                                   (nextSym is { } n && $"{sym}{n}" == Environment.NewLine))
            ? ScanResult.Token(TokenType.Separator)
            : ScanResult.Nothing();
    }
}