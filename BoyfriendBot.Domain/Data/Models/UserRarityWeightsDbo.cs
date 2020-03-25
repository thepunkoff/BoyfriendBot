using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BoyfriendBot.Domain.Data.Models
{
    public class UserRarityWeightsDbo
    {
        [Key] public long UserId { get; set; }
        public int WhiteWeight { get; set; }
        public int GreenWeight { get; set; }
        public int BlueWeight { get; set; }
        public int PurpleWeight { get; set; }
        public int OrangeWeight { get; set; }

        public int GetCapacity()
        {
            return WhiteWeight + GreenWeight + BlueWeight + PurpleWeight + OrangeWeight;
        }

        public List<(MessageRarity Rarity, int Weight)> GetList()
        {
            return new List<(MessageRarity, int)>
            {
                (MessageRarity.WHITE, WhiteWeight),
                (MessageRarity.GREEN, GreenWeight),
                (MessageRarity.BLUE, BlueWeight),
                (MessageRarity.PURPLE, PurpleWeight),
                (MessageRarity.ORANGE, OrangeWeight),
            };
        }
    }
}
