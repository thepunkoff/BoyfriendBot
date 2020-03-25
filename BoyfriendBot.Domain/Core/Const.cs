using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Core
{
    public class Const
    {
        public const string ErrorMessage = "Упс, произошло какая-то ошибка.";
        public const string RedAlertMessage = "Ты мне нравишься.";
        public class PartOfDay
        {
            public const string Night = "Night";
            public const string Morning = "Morning";
            public const string Afternoon = "Afternoon";
            public const string Evening = "Evening";
        }

        public class XmlAliases
        {
            public const string RarityAttribute = "rarity";
            public const string TypeAttribute = "type";

            public const string WakeUpCategory = "WakeUp";
        }

        public class CommandPatterns
        {
            public const string SettingsCommand = "/settings";
        }
    }
}
