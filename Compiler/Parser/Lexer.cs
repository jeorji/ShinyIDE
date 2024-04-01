using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;

namespace Compiler.parser;

using LexemeEater = Func<Caret.Eater, LexemeType?>;

public class Lexer
{
    private static readonly LexemeEater[] LexemeEaters =
    [
        TryEatFnKeyword,
        TryEatI32Keyword,
        TryEatComma,
        TryEatColon,
        TryEatGreater,
        TryEatMinus,
        TryEatReturnKeyword,
        TryEatSemicolon,
        TryEatSeparator,
        TryEatIdentifier,
        TryEatOpenBracket,
        TryEatOpenFnBody,
        TryEatCloseFnBody,
        TryEatCloseBracket,
    ];

    public static IEnumerable<Lexeme> Scan(string content)
    {
        var caret = new Caret(content);
        var lexemes = new List<Lexeme>();

        while (!caret.IsEnd()) lexemes.Add(OnNextIteration(caret));

        return lexemes;
    }

    private static List<Lexeme> OnNextIteration(Caret caret)
    {
        foreach (var lexemeEater in LexemeEaters)
        {
            var newLexemes = EatLexeme(caret, lexemeEater);
            if (newLexemes.Count > 0) return newLexemes;
        }

        caret.Move();
        return [];
    }

    private static List<Lexeme> EatLexeme(Caret caret, Func<Caret.Eater, LexemeType?> eatFunc)
    {
        var eater = caret.StartEating();

        try
        {
            return eatFunc(eater) is { } lexemeType
                ? HandleEatingResult(caret.FinishEating(eater), lexemeType)
                : [];
        }
        catch (EatException ex)
        {
            return [ex.Error];
        }
    }

    private static List<Lexeme> HandleEatingResult(Caret.EatingResult eatingResult, LexemeType lexemeType)
    {
        var lexeme = lexemeType.IntoLexeme(eatingResult.NewSpan);

        if (eatingResult.OldSpan.IsNotEmpty())
            return [InvalidLexemeType.UnexpectedSymbol.IntoLexeme(eatingResult.OldSpan), lexeme];

        return [lexeme];
    }

    private class EatException : Exception
    {
        public Lexeme.Invalid Error { get; init; }
    }

    private static LexemeType? TryEatIdentifier(Caret.Eater eater)
    {
        if (!eater.Eat(IsIdentifierHead)) return null;
        eater.EatWhile(IsIdentifierTail);

        return LexemeType.Identifier;
    }

    private static bool IsIdentifierHead(char sym)
    {
        return char.IsLetter(sym) || sym == '_';
    }

    private static bool IsIdentifierTail(char sym)
    {
        return char.IsLetterOrDigit(sym) || sym == '_';
    }

    private static LexemeType? TryEatFnKeyword(Caret.Eater eater)
    {
        return eater.Eat("fn") ? LexemeType.FnKeyword : null;
    }
    private static LexemeType? TryEatI32Keyword(Caret.Eater eater)
    {
        return eater.Eat("i32") ? LexemeType.I32Keyword : null;
    }
    private static LexemeType? TryEatReturnKeyword(Caret.Eater eater)
    {
        return eater.Eat("return") ? LexemeType.ReturnKeyword : null;
    }

    private static LexemeType? TryEatColon(Caret.Eater eater)
    {
        return eater.Eat(':') ? LexemeType.Colon : null;
    }
    private static LexemeType? TryEatOpenFnBody(Caret.Eater eater)
    {
        return eater.Eat('{') ? LexemeType.OpenFnBody : null;
    }
    private static LexemeType? TryEatCloseFnBody(Caret.Eater eater)
    {
        return eater.Eat('}') ? LexemeType.CloseFnBody : null;
    }
    private static LexemeType? TryEatOpenBracket(Caret.Eater eater)
    {
        return eater.Eat('(') ? LexemeType.OpenBracket : null;
    }
    private static LexemeType? TryEatCloseBracket(Caret.Eater eater)
    {
        return eater.Eat(')') ? LexemeType.CloseBracket : null;
    }
    private static LexemeType? TryEatComma(Caret.Eater eater)
    {
        return eater.Eat(',') ? LexemeType.Comma : null;
    }

    private static LexemeType? TryEatMinus(Caret.Eater eater)
    {
        return eater.Eat('-') ? LexemeType.Minus : null;
    }
    private static LexemeType? TryEatGreater(Caret.Eater eater)
    {
        return eater.Eat('>') ? LexemeType.Greater : null;
    }
    private static LexemeType? TryEatSemicolon(Caret.Eater eater)
    {
        return eater.Eat(';') ? LexemeType.Semicolon : null;
    }

    private static LexemeType? TryEatSeparator(Caret.Eater eater)
    {
        return eater.EatWhile(IsSeparator) ? LexemeType.Separator : null;
    }

    private static bool IsSeparator(char sym, char? nextSym)
    {
        return char.IsSeparator(sym)
               || $"{sym}" == Environment.NewLine
               || (nextSym is { } n && $"{sym}{n}" == Environment.NewLine);
    }
}