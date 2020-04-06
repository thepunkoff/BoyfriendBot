using BoyfriendBot.Domain.Services.Models;
using System;
using System.Linq.Expressions;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IExpressionBuilder
    {
        Func<string, bool> BuildCondition(MatchCategory matchCategory);
    }
}
