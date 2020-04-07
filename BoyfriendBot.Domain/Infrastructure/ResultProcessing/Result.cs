using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Infrastructure.ResultProcessing
{
    public class Result<T>
    {
        public T Value { get; set; }
        public string Message { get; set; }

        public Result(T value)
        {
            Value = value;
        }

        public Result(string message)
        {
            Message = message;
        }
    }
}
