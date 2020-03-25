using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace BoyfriendBot.WebApp.Controllers
{
    [ApiController]
    public class BoyfriendBotController : Controller
    {
        private readonly ILogger<BoyfriendBotController> _logger;
        private readonly IMonitoringManager _monitoringManager;
        private readonly IEventManager _eventManager;

        public BoyfriendBotController(
              ILogger<BoyfriendBotController> logger
            , IMonitoringManager monitoringManager
            , IEventManager eventManager
            )
        {
            _logger = logger;
            _monitoringManager = monitoringManager;
            _eventManager = eventManager;
        }

        [HttpGet]
        [Route("monitor")]
        public ActionResult Monitor()
        {
            var monitoring = new MonitorViewModel
            {
                Description = "online",
                Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                Listening = _monitoringManager.Listening,
                SchedulingMessages = _monitoringManager.SchedulingMessages,
                TotalUsers = _monitoringManager.GetTotalUsers()
            };

            return View("Monitor", monitoring);
        }

        [HttpGet]
        [Route("reschedule")]
        public ActionResult Reschedule()
        {
            _eventManager.InvokeRescheduleClicked();
            return Redirect("~/monitor");
        }
    }
}
