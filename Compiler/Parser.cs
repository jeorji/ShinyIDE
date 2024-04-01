using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Optional;
using Scanner;

namespace Compiler;

using Token = Token<TokenType, TokenError>;

public enum State
{
    Start,
    Token,
    End
}

public enum ParseErrorType
{
    ConstKeywordExpected,
    IdentifierExpected,
    TypeDividerExpected,
    LinkExpected,
    StrTypeExpected,
    AssignmentOperatorExpected,
    StringLiteralExpected,
    OperatorEndExpected,
    NothingExpected,
    SeparatorExpected
}

public struct ParseError
{
    public ParseErrorType Type { get; internal init; }

    public Span Span { get; internal init; }
}

public class Parser
{
    private readonly IEnumerable<Token> _tokens;

    private State _state;
    private TokenType _tokenType;
    private Option<ParseErrorType> _error;
    private Span _span;

    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens;
        _state = State.Start;
        _tokenType = default;
        _error = Option.None<ParseErrorType>();
        _span = new Span();
    }

    public Option<ParseError> Parse()
    {
        _state = State.Start;
        _tokenType = default;
        _error = Option.None<ParseErrorType>();
        _span = new Span();

        using var tokens = _tokens.GetEnumerator();
        tokens.MoveNext();

        while (true)
        {
            Token.ValidToken token;
            try
            {
                if (tokens.Current is not Token.ValidToken t) return Option.None<ParseError>();
                // tokens.MoveNext();
                // continue;
                token = t;
            }
            catch (Exception)
            {
                break;
            }

            var nextToken = tokens.MoveNext()
                ? tokens.Current.Some()
                : Option.None<Token>();

            if (token.Type == TokenType.Separator) continue;

            var nextValidToken = nextToken
                .FlatMap(t => t is Token.ValidToken v
                    ? v.Some()
                    : Option.None<Token.ValidToken>()
                );

            foreach (var error in SwitchState(token, nextValidToken)) return error.Some();

            if (!nextToken.HasValue) break;
        }

        foreach (var error in SwitchState()) return error.Some();
        return Option.None<ParseError>();
    }

    private Option<ParseError> SwitchState(Token.ValidToken token, Option<Token.ValidToken> nextToken)
    {
        return SwitchState(Option.Some(token), nextToken, false);
    }

    private Option<ParseError> SwitchState()
    {
        return SwitchState(Option.None<Token.ValidToken>(), Option.None<Token.ValidToken>(), true);
    }

    private Option<ParseError> SwitchState(Option<Token.ValidToken> token, Option<Token.ValidToken> nextToken,
        bool isLast)
    {
        _error = Option.None<ParseErrorType>();

        OnNextTokenType(token.Map(t => t.Type), nextToken.Map(t => t.Type));
        token.MatchSome(t => _span = t.Span);

        foreach (var error in _error)
            return new ParseError { Type = error, Span = isLast ? new Span(_span.End, _span.End) : _span }.Some();
        return Option.None<ParseError>();
    }

    private void OnNextTokenType(Option<TokenType> nextTokenType, Option<TokenType> nextNextTokenType)
    {
        if (_state is State.Start)
        {
            _state = State.Token;
            _error = nextTokenType.Contains(TokenType.ConstKeyword) && nextNextTokenType.Contains(TokenType.Separator)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.ConstKeywordExpected.Some();
            _tokenType = _error.HasValue ? _tokenType : TokenType.ConstKeyword;

            // _error = nextTokenType.Contains(TokenType.ConstKeyword)
            //     ? nextNextTokenType.Contains(TokenType.Separator)
            //         ? Option.None<ParseErrorType>()
            //         : ParseErrorType.SeparatorExpected.Some()
            //     : ParseErrorType.ConstKeywordExpected.Some();
            // _tokenType = TokenType.ConstKeyword;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.ConstKeyword)
        {
            _error = nextTokenType.Contains(TokenType.Identifier)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.IdentifierExpected.Some();
            // _tokenType = _error.HasValue ? _tokenType : TokenType.Identifier;
            _tokenType = TokenType.Identifier;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.Identifier)
        {
            _error = nextTokenType.Contains(TokenType.Colon)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.TypeDividerExpected.Some();
            // _tokenType = _error.HasValue ? _tokenType : TokenType.Colon;
            _tokenType = TokenType.Colon;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.Colon)
        {
            _error = nextTokenType.Contains(TokenType.Ampersand)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.LinkExpected.Some();
            // _tokenType = _error.HasValue ? _tokenType : TokenType.Ampersand;
            _tokenType = TokenType.Ampersand;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.Ampersand)
        {
            _error = nextTokenType.Contains(TokenType.StrKeyword)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.StrTypeExpected.Some();
            // _tokenType = _error.HasValue ? _tokenType : TokenType.StrKeyword;
            _tokenType = TokenType.StrKeyword;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.StrKeyword)
        {
            _error = nextTokenType.Contains(TokenType.AssignmentOperator)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.AssignmentOperatorExpected.Some();
            // _tokenType = _error.HasValue ? _tokenType : TokenType.AssignmentOperator;
            _tokenType = TokenType.AssignmentOperator;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.AssignmentOperator)
        {
            _error = nextTokenType.Contains(TokenType.StringLiteral)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.StringLiteralExpected.Some();
            // _tokenType = _error.HasValue ? _tokenType : TokenType.StringLiteral;
            _tokenType = TokenType.StringLiteral;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.StringLiteral)
        {
            _error = nextTokenType.Contains(TokenType.OperatorEnd)
                ? Option.None<ParseErrorType>()
                : ParseErrorType.OperatorEndExpected.Some();
            // _tokenType = _error.HasValue ? _tokenType : TokenType.OperatorEnd;
            _tokenType = TokenType.OperatorEnd;

            return;
        }

        if (_state is State.Token && _tokenType is TokenType.OperatorEnd)
        {
            // nextTokenType.Match(
            //     t =>
            //     {
            //         if ((t == TokenType.Separator || t == TokenType.OperatorEnd) &&
            //             (nextNextTokenType.Contains(TokenType.Separator) || !nextNextTokenType.HasValue))
            //         {
            //             _state = State.End;
            //             _error = nextTokenType.HasValue && !nextTokenType.Contains(TokenType.Separator) &&
            //                      !nextTokenType.Contains(TokenType.OperatorEnd)
            //                 ? ParseErrorType.NothingExpected.Some()
            //                 : Option.None<ParseErrorType>();
            //             return;
            //         }
            //
            //         _tokenType = TokenType.ConstKeyword;
            //         // _state = State.Start;
            //         _error = t == TokenType.ConstKeyword
            //             ? nextNextTokenType.Contains(TokenType.Separator)
            //                 ? Option.None<ParseErrorType>()
            //                 : ParseErrorType.SeparatorExpected.Some()
            //             : ParseErrorType.ConstKeywordExpected.Some();
            //     },
            //     () =>
            //     {
            //         _state = State.End;
            //         _error = nextTokenType.HasValue
            //             ? ParseErrorType.NothingExpected.Some()
            //             : Option.None<ParseErrorType>();
            //     }
            // );
            _state = State.End;
            _error = nextTokenType.HasValue && !nextTokenType.Contains(TokenType.OperatorEnd)
                ? ParseErrorType.NothingExpected.Some()
                : Option.None<ParseErrorType>();

            return;
        }

        if (_state is State.End)
        {
            _error = nextTokenType.HasValue && !nextTokenType.Contains(TokenType.OperatorEnd)
                ? ParseErrorType.NothingExpected.Some()
                : Option.None<ParseErrorType>();
            return;
        }
    }
}

// public class Parser : IEnumerable<ParseError>
// {
//     private readonly IEnumerable<Token> _tokens;
//
//     private State _state;
//     private TokenType _tokenType;
//     private Option<ParseErrorType> _error;
//     private Span _span;
//
//     public Parser(IEnumerable<Token> tokens)
//     {
//         _tokens = tokens;
//         _state = State.Start;
//         _tokenType = default;
//         _error = Option.None<ParseErrorType>();
//         _span = new Span();
//     }
//
//     public IEnumerator<ParseError> GetEnumerator()
//     {
//         _state = State.Start;
//         _tokenType = default;
//         _error = Option.None<ParseErrorType>();
//         _span = new Span();
//
//         foreach (var token in _tokens)
//         {
//             if (token is not Token.ValidToken validToken) continue;
//             var nextTokenType = validToken.Type;
//
//             if (nextTokenType is TokenType.Separator) continue;
//
//             _error = Option.None<ParseErrorType>();
//             OnNextTokenType(nextTokenType.Some());
//             _span = token.Span;
//
//             foreach (var error in _error) yield return new ParseError { Type = error, Span = _span };
//         }
//
//         _error = Option.None<ParseErrorType>();
//         OnNextTokenType(Option.None<TokenType>());
//
//         foreach (var error in _error)
//             yield return new ParseError { Type = error, Span = new Span(_span.End, _span.End) };
//     }
//
//     IEnumerator IEnumerable.GetEnumerator()
//     {
//         return GetEnumerator();
//     }
//
//     private void OnNextTokenType(Option<TokenType> nextTokenType)
//     {
//         if (_state is State.Start)
//         {
//             _state = State.Token;
//             _error = nextTokenType.Contains(TokenType.ConstKeyword)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.ConstKeywordExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.ConstKeyword;
//             _tokenType = TokenType.ConstKeyword;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.ConstKeyword)
//         {
//             _error = nextTokenType.Contains(TokenType.Identifier)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.IdentifierExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.Identifier;
//             _tokenType = TokenType.Identifier;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.Identifier)
//         {
//             _error = nextTokenType.Contains(TokenType.Colon)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.TypeDividerExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.Colon;
//             _tokenType = TokenType.Colon;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.Colon)
//         {
//             _error = nextTokenType.Contains(TokenType.Ampersand)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.LinkExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.Ampersand;
//             _tokenType = TokenType.Ampersand;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.Ampersand)
//         {
//             _error = nextTokenType.Contains(TokenType.StrKeyword)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.StrTypeExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.StrKeyword;
//             _tokenType = TokenType.StrKeyword;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.StrKeyword)
//         {
//             _error = nextTokenType.Contains(TokenType.AssignmentOperator)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.AssignmentOperatorExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.AssignmentOperator;
//             _tokenType = TokenType.AssignmentOperator;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.AssignmentOperator)
//         {
//             _error = nextTokenType.Contains(TokenType.StringLiteral)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.StringLiteralExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.StringLiteral;
//             _tokenType = TokenType.StringLiteral;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.StringLiteral)
//         {
//             _error = nextTokenType.Contains(TokenType.OperatorEnd)
//                 ? Option.None<ParseErrorType>()
//                 : ParseErrorType.OperatorEndExpected.Some();
//             // _tokenType = _error.HasValue ? _tokenType : TokenType.OperatorEnd;
//             _tokenType = TokenType.OperatorEnd;
//
//             return;
//         }
//
//         if (_state is State.Token && _tokenType is TokenType.OperatorEnd)
//         {
//             _state = State.End;
//             _error = nextTokenType.HasValue
//                 ? ParseErrorType.NothingExpected.Some()
//                 : Option.None<ParseErrorType>();
//
//             return;
//         }
//
//         if (_state is State.End)
//         {
//             _error = nextTokenType.HasValue
//                 ? ParseErrorType.NothingExpected.Some()
//                 : Option.None<ParseErrorType>();
//             return;
//         }
//     }
// }