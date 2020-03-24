using BoyfriendBot.Domain.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Core
{
    public struct DateTimeRange : IRange<DateTime>
    {
        public DateTimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public TimeSpan Difference => End - Start;

        public bool Includes(DateTime value)
        {
            return (Start <= value) && (value <= End);
        }

        public bool Includes(IRange<DateTime> range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }
    }
}
