using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IInputProcessor
    {
        Task ProcessUserInput(string userInput, long chatId);
    }
}
