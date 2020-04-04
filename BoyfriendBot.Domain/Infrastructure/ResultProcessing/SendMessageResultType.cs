using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Infrastructure.ResultProcessing
{
    public enum SendMessageResultType
    {
        SUCCESS = 0,
        BLOCKED = 1,
        UNKNOWN = 2
    }
}
