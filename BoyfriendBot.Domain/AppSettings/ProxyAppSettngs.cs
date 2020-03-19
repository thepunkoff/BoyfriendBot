using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.AppSettings
{
    public class ProxyAppSettngs
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
