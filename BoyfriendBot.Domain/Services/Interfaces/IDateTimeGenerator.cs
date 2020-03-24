using BoyfriendBot.Domain.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IDateTimeGenerator
    {
        DateTime GenerateRandomDateTimeWithinRange(DateTimeRange range);
        List<DateTime> GenerateDateTimesWithinRange(TimeSpanRange range, int messageCount);
    }
}
