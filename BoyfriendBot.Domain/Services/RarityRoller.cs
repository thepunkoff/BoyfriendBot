using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.Domain.Services.Models;
using System;
namespace BoyfriendBot.Domain.Services
{
    public class RarityRoller : IRarityRoller
    {
        public MessageRarity RollRarityForUser(UserDbo userDbo)
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
