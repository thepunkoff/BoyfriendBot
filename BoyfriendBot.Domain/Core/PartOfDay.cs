using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Core
{
    public struct PartOfDay
    {
        private static Dictionary<PartOfDay, TimeSpanRange> _ranges { get; } = new Dictionary<PartOfDay, TimeSpanRange>
        {
            [Night] = new TimeSpanRange(TimeSpan.FromHours(0), new TimeSpan(4, 59, 59)),
            [Morning] = new TimeSpanRange(TimeSpan.FromHours(5), new TimeSpan(11, 59, 59)),
            [Afternoon] = new TimeSpanRange(TimeSpan.FromHours(12), new TimeSpan(17, 59, 59)),
            [Evening] = new TimeSpanRange(TimeSpan.FromHours(18), new TimeSpan(23, 59, 59)),
        };

        public string Name { get; set; }

        public TimeSpan Start => _ranges[this].Start;
        public TimeSpan End => _ranges[this].End;

        public static PartOfDay Night => new PartOfDay { Name = Const.PartOfDay.Night };

        public static PartOfDay Morning => new PartOfDay { Name = Const.PartOfDay.Morning };

        public static PartOfDay Afternoon => new PartOfDay { Name = Const.PartOfDay.Afternoon };

        public static PartOfDay Evening => new PartOfDay { Name = Const.PartOfDay.Evening };
        
        public PartOfDay Next
        {
            get
            {
                if (Name == Const.PartOfDay.Night) return Morning;

                else if (Name == Const.PartOfDay.Morning) return Afternoon;

                else if (Name == Const.PartOfDay.Afternoon) return Evening;

                else return Night;
            }
        }

        public List<PartOfDay> Rest
        {
            get
            {
                var list = new List<PartOfDay>();

                var p = this;

                while (p != Night)
                {
                    p = p.Next;
                    list.Add(p);
                }

                return list;
            }
        }

        public List<PartOfDay> Past
        {
            get
            {
                var list = new List<PartOfDay>();

                var p = Evening;

                while (p != this)
                {
                    p = p.Next;
                    list.Add(p);
                }

                return list;
            }
        }

        public static PartOfDay GetPartOfDay(DateTime dateTime)
        {
            var timeOfDay = dateTime.TimeOfDay;

            foreach (var range in _ranges)
            {
                if (_ranges[range.Key].Includes(timeOfDay))
                {
                    return new PartOfDay { Name = range.Key.Name };
                }
            }

            throw new ApplicationException($"Couldn't find a part of day for {timeOfDay}");
        }

        public override bool Equals(object obj)
        {
            var other = (PartOfDay)obj;

            return Name == other.Name;
        }

        public static bool operator ==(PartOfDay obj1, PartOfDay obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(PartOfDay obj1, PartOfDay obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
