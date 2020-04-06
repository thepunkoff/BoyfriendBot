using BoyfriendBot.Domain.Services.Models;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IStringAnalyzer
    {
        bool IsMatch(string input, MatchCategory matchCategory);
    }
}
