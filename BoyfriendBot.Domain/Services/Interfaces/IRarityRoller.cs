using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IRarityRoller
    {
        MessageRarity RollRarityForUser(UserDbo userDbo);
    }
}
