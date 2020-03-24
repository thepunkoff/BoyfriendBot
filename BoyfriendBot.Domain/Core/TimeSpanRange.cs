using BoyfriendBot.Domain.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Core
{
    public struct TimeSpanRange : IRange<TimeSpan>
    {
        public TimeSpanRange(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        public TimeSpan Start { get; private set; }
        public TimeSpan End { get; private set; }

        public TimeSpan Difference => End - Start;

        public bool Includes(TimeSpan value)
        {
            return (Start <= value) && (value <= End);
        }

        public bool Includes(IRange<TimeSpan> range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }
    }
}
