using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public class RarityRoller : IRarityRoller
    {
        private readonly IUserStorage _userStorage;

        public RarityRoller(
              IUserStorage userStorage
            )
        {
            _userStorage = userStorage;
        }

        public MessageRarity RollRarityForUser(UserDbo user)
        {
            return RollRarity(user);
        }

        public async Task<MessageRarity> RollRarityForUser(long chatId)
        {
            var userDbo = await _userStorage.GetUserByChatIdNoTracking(chatId);

            return RollRarity(userDbo);
        }

        private MessageRarity RollRarity(UserDbo userDbo)
        {
            MessageRarity winner = default;

            var weights = userDbo.RarityWeights;

            var capacity = weights.GetCapacity();

            var rng = new Random();
            var pointer = rng.Next(capacity);

            var rarities = userDbo.RarityWeights.GetList();

            var sum = 0;
            for (int i = 0; i < rarities.Count; i++)
            {
                sum += rarities[i].Weight;

                if (sum >= pointer)
                {
                    winner = rarities[i].Rarity;
                    break;
                }
            }

            return winner;
        }
    }
}
