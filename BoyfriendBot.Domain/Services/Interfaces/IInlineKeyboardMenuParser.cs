using BoyfriendBot.Domain.Services.Models;
namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IInlineKeyboardMenuParser
    {
        InlineKeyboardMenu Parse(string menuId);
    }
}
