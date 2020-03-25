using BoyfriendBot.Domain.Data.Models;
using System;

namespace BoyfriendBot.Domain.Services.Interfaces
{
    public interface IEventManager
    {
        event EventHandler<UserDbo> NewUserEvent;
        public event EventHandler RescheduleClickedEvent;

        void InvokeNewUser(UserDbo user);
        void InvokeRescheduleClicked();

    }
}
