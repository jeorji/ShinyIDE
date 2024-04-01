using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Animation.Easings;
using Scanner;

namespace Compiler;

public class Fixer
{
    private readonly string _content;
    private readonly Token<TokenType, TokenError>[] _tokens;

    public Fixer(string content)
    {
        _content = content;
        _tokens = new Scanner<TokenType, TokenError>(content, TokensScanners.TokenScanners).ToArray();
    }

    public string Fix()
    {
        var content = FixTokenErrors();

        while (true)
        {
            var (stop, newContent) = FixParserErrors(content);
            if (stop) break;

            content = newContent;
        }

        return content;
    }

    private (bool, string) FixParserErrors(string content)
    {
        var tokens = new Scanner<TokenType, TokenError>(content, TokensScanners.TokenScanners);
        var parser = new Parser(tokens);

        foreach (var parseError in parser.Parse())
        {
            var replacement = parseError.Type switch
            {
                ParseErrorType.ConstKeywordExpected => "const",
                ParseErrorType.IdentifierExpected => "change_me",
                ParseErrorType.TypeDividerExpected => ":",
                ParseErrorType.LinkExpected => "&",
                ParseErrorType.StrTypeExpected => "str",
                ParseErrorType.AssignmentOperatorExpected => "=",
                ParseErrorType.StringLiteralExpected => "\"change me\"",
                ParseErrorType.OperatorEndExpected => ";",
                ParseErrorType.NothingExpected => "",
                ParseErrorType.SeparatorExpected => " "
            };

            if (parseError.Type is ParseErrorType.NothingExpected)
                return (false,
                    content.Remove(parseError.Span.Start, parseError.Span.End - parseError.Span.Start)
                        .Insert(parseError.Span.Start, replacement));

            // if (parseError.Type is ParseErrorType.TypeDividerExpected || parseError.Type is ParseErrorType.LinkExpected || parseError.Type is ParseErrorType.LinkExpected)
            // return (false,
            //     content
            //         .Insert(parseError.Span.Start, replacement));

            return (false, content.Remove(parseError.Span.Start, parseError.Span.End - parseError.Span.Start)
                .Insert(parseError.Span.Start, replacement));
            // return (false, content.Insert(parseError.Span.End, replacement));
        }

        return (true, content);
    }

    private string FixTokenErrors()
    {
        var validTokens = _tokens
            .Select(token => token as Token<TokenType, TokenError>.ValidToken)
            .Where(t => t is not null)
            .Cast<Token<TokenType, TokenError>.ValidToken>().ToArray();

        return string.Join(
            "",
            validTokens.Select(t => _content.Substring(t.Span.Start, t.Span.End - t.Span.Start))
        );
    }
}