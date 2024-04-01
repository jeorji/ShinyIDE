namespace Scanner;

public class Caret
{
    private readonly string _content;
    private Span _span;

    internal Caret Clone()
    {
        return new Caret(_content)
        {
            _span = _span
        };
    }

    internal Caret(string content)
    {
        _content = content;
    }

    private bool IsOutOfRange()
    {
        return _span.End >= _content.Length;
    }

    private bool IsOutOfRange(int shift)
    {
        return _span.End + shift >= _content.Length;
    }

    private char? GetCurrentSymbol()
    {
        if (IsOutOfRange()) return null;
        return _content[_span.End];
    }

    private char? GetNextSymbol()
    {
        if (IsOutOfRange(1)) return null;
        return _content[_span.End + 1];
    }

    public bool TryEat(char symbol)
    {
        if (GetCurrentSymbol() != symbol) return false;

        _span.MoveEndPos();
        return true;
    }

    public bool TryEat(Func<char, bool> predicate)
    {
        while (GetCurrentSymbol() is { } currentSymbol && predicate(currentSymbol))
        {
            _span.MoveEndPos();
            return true;
        }

        return false;
    }

    public bool TryEat(char symbol, char nextSymbol)
    {
        if (GetCurrentSymbol() != symbol || GetNextSymbol() != nextSymbol) return false;

        _span.MoveEndPos();
        _span.MoveEndPos();

        return true;
    }

    public bool TryEatWhile(Func<char, bool> predicate)
    {
        var start = _span.End;
        while (GetCurrentSymbol() is { } currentSymbol && predicate(currentSymbol)) _span.MoveEndPos();
        return _span.End - start > 0;
    }

    public bool TryEatWhile(Func<char, char?, bool> predicate)
    {
        var start = _span.End;
        while (GetCurrentSymbol() is { } currentSymbol &&
               predicate(currentSymbol, GetNextSymbol())) _span.MoveEndPos();
        return _span.End - start > 0;
    }

    internal void Eat()
    {
        _span.MoveEndPos();
    }

    public string Slice()
    {
        // if (IsOutOfRange()) return "";
        return _content[_span.Start..int.Min(_span.End, _content.Length)];
        // return _content[_span.Start.._span.End];
    }

    internal Span Span()
    {
        return _span;
    }

    internal void Reset()
    {
        _span.Reset();
    }

    internal void ResetStartPos()
    {
        _span.ResetStartPos();
    }

    internal bool IsEnd()
    {
        return _span.End == _content.Length;
    }
}