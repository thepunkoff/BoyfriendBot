using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Models;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IRarityRoller
    {
        MessageRarity RollRarityForUser(UserDbo user);

        Task<MessageRarity> RollRarityForUser(long chatId);
    }
}
