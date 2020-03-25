using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface ITelegramClient
    {
        Task SendMessageAsync(string category, MessageType type, MessageRarity rarity, long chatId);
    }
}
