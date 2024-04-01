using System;

namespace Compiler.Parser;

public readonly struct Span
{
    public int Start { get; private init; }

    public int End { get; private init; }

    public Span() : this(0, 0)
    {
    }

    public Span(int start, int end)
    {
        if (start < 0) throw new ArgumentException("'start' must be non-negative");
        if (end < 0) throw new ArgumentException("'end' must be non-negative");
        if (start > end) throw new ArgumentException("'start' must be less than or equal to the 'end'");

        Start = start;
        End = end;
    }

    public bool IsEmpty()
    {
        return End <= Start;
    }

    public bool IsNotEmpty()
    {
        return !IsEmpty();
    }

    public Span ShiftStartToEnd()
    {
        return new Span
        {
            Start = End,
            End = End
        };
    }

    public Span ShiftEndToStart()
    {
        return new Span
        {
            Start = Start,
            End = Start
        };
    }

    public Span ShiftEnd(int shift)
    {
        if (shift < 1) throw new ArgumentException("'start' must be positive");

        return new Span
        {
            Start = Start,
            End = End + shift
        };
    }

    public Span ShiftStart(int shift)
    {
        if (shift < 1) throw new ArgumentException("'start' must be positive");
        if (Start + shift > End) throw new ArgumentException("shifted 'Start' must be less than or equal to the 'End'");

        return new Span
        {
            Start = Start + shift,
            End = End
        };
    }
}