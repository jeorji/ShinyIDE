namespace Compiler.Parser;

public enum LexemeType
{
    FnKeyword = 1,
    I32Keyword = 2,
    ReturnKeyword = 3,
    Colon = 4,
    Comma = 5,
    OpenBracket = 6,
    CloseBracket = 7,
    OpenFnBody = 8,
    CloseFnBody = 9,
    Minus = 10,
    Greater = 11,
    Identifier = 12,
    Semicolon = 13,
    Separator = 14
}