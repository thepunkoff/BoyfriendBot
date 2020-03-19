using BoyfriendBot.Domain.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IUserStorage
    {
        bool HasUser(long userId);
        Task AddNewUser(long userId, long chatId);
        Task AddNewUser(UserDbo userDbo);
    }
}
