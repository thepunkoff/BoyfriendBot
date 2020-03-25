using BoyfriendBot.Domain.Data.Models;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Services
{
    public class EventManager : IEventManager
    {
        private readonly ILogger<EventManager> _logger;

        public event EventHandler<UserDbo> NewUserEvent;
        public event EventHandler RescheduleClickedEvent;

        public EventManager(
              ILogger<EventManager> logger
            )
        {
            _logger = logger;
        }

        public void InvokeNewUser(UserDbo user)
        {
            _logger.LogInformation($"New user has joined: {user.ToString()}");

            NewUserEvent?.Invoke(this, user);
        }

        public void InvokeRescheduleClicked()
        {
            _logger.LogInformation($"\"Reschedule\" was clicked on monitor page");

            RescheduleClickedEvent?.Invoke(this, null);
        }

    }
}
