using BoyfriendBot.Domain.Services.Interfaces;
using BoyfriendBot.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace BoyfriendBot.WebApp.Controllers
{
    public class BoyfriendBotController : Controller
    {
        private readonly ILogger<BoyfriendBotController> _logger;
        private readonly IMonitoringManager _monitoringManager;
        private readonly IEventManager _eventManager;
        private readonly IUserStorage _userStorage;

        public BoyfriendBotController(
              ILogger<BoyfriendBotController> logger
            , IMonitoringManager monitoringManager
            , IEventManager eventManager
            , IUserStorage userStorage
            )
        {
            _logger = logger;
            _monitoringManager = monitoringManager;
            _eventManager = eventManager;
            _userStorage = userStorage;
        }

        [HttpGet]
        public IActionResult Monitor()
        {
            var monitoring = new MonitorViewModel
            {
#if RELEASE
                Description = "prod",
#else
                Description = "dev",
#endif
                Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime,
                Listening = _monitoringManager.Listening,
                SchedulingMessages = _monitoringManager.SchedulingMessages,
                TotalUsers = _userStorage.GetTotalUsers()
            };

            ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version;

            return View("Monitor", monitoring);
        }

        [HttpGet]
        public IActionResult Reschedule()
        {
            _eventManager.InvokeRescheduleClicked();
            return RedirectToAction("Monitor");
        }

        [HttpPost]
        public IActionResult Auth(string username, string password)
        {
            if (username == "admin" && password == "admin")
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
