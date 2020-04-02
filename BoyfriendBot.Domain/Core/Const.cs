using BoyfriendBot.Domain.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Core
{
    public class Const
    {
        public const string ErrorMessage = "Упс, произошла какая-то ошибка.";
        public const string RedAlertMessage = "Ты мне нравишься.";

        public class Gender
        {
            public const bool Male = true;
            public const bool Female = false;
        }

        public class Commands
        {
            public const string SendMenuCommand = "menu";
            public const string SetSettingCommand = "set";

            public static Dictionary<string, string> CommandAliases { get; } = new Dictionary<string, string>
            {
                ["settings"] = "menu settings_main"
            };
        }
        public class PartOfDay
        {
            public const string Night = "Night";
            public const string Morning = "Morning";
            public const string Afternoon = "Afternoon";
            public const string Evening = "Evening";

            public static Dictionary<Core.PartOfDay, List<(string Type, TimeSpanRange Range)>> SpecialMessageTypes { get; } = new Dictionary<Core.PartOfDay, List<(string, TimeSpanRange)>>
            {
                [Core.PartOfDay.Night] = new List<(string, TimeSpanRange)> { (null, default) },
                [Core.PartOfDay.Morning] = new List<(string, TimeSpanRange)> {
                    (XmlAliases.GoodMorningType, new TimeSpanRange(new TimeSpan(8, 0, 0), new TimeSpan(12, 0, 0)))
                },
                [Core.PartOfDay.Afternoon] = new List<(string, TimeSpanRange)> {
                    (XmlAliases.LunchType, new TimeSpanRange(new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)))
                },
                [Core.PartOfDay.Evening] = new List<(string, TimeSpanRange)> {
                    (XmlAliases.DinnerType, new TimeSpanRange(new TimeSpan(19, 0, 0), new TimeSpan(21, 0, 0))),
                    (XmlAliases.GoodNightType, new TimeSpanRange(new TimeSpan(22, 0, 0), new TimeSpan(23, 59, 59))),
                }
            };
        }

        public class XmlAliases
        {
            public const string RarityAttribute = "rarity";
            public const string TypeAttribute = "type";

            public const string WakeUpCategory = "WakeUp";

            public const string GoodMorningType = "goodmorning";
            public const string LunchType = "lunch";
            public const string DinnerType = "dinner";
            public const string GoodNightType = "goodnight";
        }

        public class CommandPatterns
        {
            public const string SettingsCommand = "/settings";
        }
    }
}
