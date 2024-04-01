using System;
using Compiler.Parser;
//using Scanner;

namespace Compiler.ViewModels;

public class EditorTokenViewModel(CaretPos caretPos, Lexeme lexeme, string content) : ViewModelBase
{
    public Lexeme Lexeme { get; } = lexeme;

    public CaretPos CaretPos { get; } = caretPos;
    public string Code => Lexeme is Lexeme.Valid v ? ((int)v.Type).ToString() : "";
    public string Value => content[Lexeme.Span.Start..Lexeme.Span.End];
}