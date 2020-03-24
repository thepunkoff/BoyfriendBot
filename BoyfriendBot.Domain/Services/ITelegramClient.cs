﻿using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services
{
    public interface ITelegramClient
    {
        Task SendMessageAsync(PartOfDay partOfDay, MessageType type, long chatId);
    }
}
