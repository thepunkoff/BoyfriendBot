using System;

namespace BoyfriendBot.Domain.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static PartOfDay PartOfDay(this DateTime dateTime)
        {
            return Core.PartOfDay.GetPartOfDay(dateTime);
        }
    }
}
