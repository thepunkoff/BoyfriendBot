using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Infrastructure.ResultProcessing
{
    public class SendMessageResult
    {
        public SendMessageResultType Type { get; set; }
        public string Message { get; set; }

        public static SendMessageResult CreateSuccess()
        {
            var result = new SendMessageResult
            {
                Type = SendMessageResultType.SUCCESS
            };

            return result;
        }

        public static SendMessageResult CreateBlocked(string message = null)
        {
            var result = new SendMessageResult
            {
                Type = SendMessageResultType.BLOCKED,
                Message = message
            };

            return result;
        }

        public static SendMessageResult CreateUnknown(string message = null)
        {
            var result = new SendMessageResult
            {
                Type = SendMessageResultType.UNKNOWN,
                Message = message
            };

            return result;
        }
    }
}
