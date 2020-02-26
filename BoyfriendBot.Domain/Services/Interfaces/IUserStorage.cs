using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IUserStorage
    {
        bool HasUser(string userName);
        Task AddNewUser(string userName, long chatId);
    }
}
