namespace Scanner;

public struct Span
{
    public int Start { get; private set; }

    public int End { get; private set; }

    public Span()
    {
        Start = 0;
        End = 0;
    }

    public Span(int start, int end)
    {
        Start = start;
        End = end;
    }

    internal bool IsEmpty()
    {
        return End <= Start;
    }

    internal bool IsNotEmpty()
    {
        return !IsEmpty();
    }

    internal void MoveEndPos()
    {
        End++;
    }

    internal void ResetStartPos()
    {
        Start = End;
    }

    internal void Reset()
    {
        Start = End = 0;
    }
}