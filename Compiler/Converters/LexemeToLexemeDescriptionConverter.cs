using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Compiler.Parser;

namespace Compiler.Converters;

public class LexemeToLexemeDescriptionConverter : IValueConverter
{
    private static IDictionary<LexemeType, string> TokenTypes = new Dictionary<LexemeType, string>
    {
        { LexemeType.CloseBracket, Lang.Resources.TokenTypeCloseBracket },
        { LexemeType.CloseFnBody, Lang.Resources.TokenTypeCloseFnBody},
        { LexemeType.Colon, Lang.Resources.TokenTypeColon },
        { LexemeType.Comma, Lang.Resources.TokenTypeComma},
        { LexemeType.FnKeyword, Lang.Resources.TokenTypeFnKeyword },
        { LexemeType.Greater, Lang.Resources.TokenTypeGreater},
        { LexemeType.I32Keyword, Lang.Resources.TokenTypeI32Keyword },
        { LexemeType.Identifier, Lang.Resources.TokenTypeIdentifier },
        { LexemeType.Minus, Lang.Resources.TokenTypeMinus},
        { LexemeType.OpenBracket, Lang.Resources.TokenTypeOpenBracket},
        { LexemeType.OpenFnBody, Lang.Resources.TokenTypeOpenFnBody},
        { LexemeType.ReturnKeyword, Lang.Resources.TokenTypeReturnKeyword},
        { LexemeType.Semicolon, Lang.Resources.TokenTypeSemiColon},
        { LexemeType.Separator, Lang.Resources.TokenTypeSeparator },
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Lexeme token) return BindingNotification.UnsetValue;

        return token switch
        {
            Lexeme.Valid validToken => TokenTypes[validToken.Type],
            Lexeme.Invalid => "invalid token"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingNotification.UnsetValue;
    }
}