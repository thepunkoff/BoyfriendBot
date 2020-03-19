using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Data.Context.Interfaces;
using BoyfriendBot.Domain.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Data.Context
{
    public class BoyfriendBotDbContext : DbContext, IBoyfriendBotDbContext
    {
        private readonly DatabaseAppSettings _appSettings;

        public BoyfriendBotDbContext(IOptions<DatabaseAppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public DbSet<UserDbo> User { get; set; }
        public DbSet<UserSettingsDbo> UserSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = _appSettings.BoyfriendBotDatabaseAbsolutePath;
            var connectionString = $"Data Source={path}";

            optionsBuilder.UseSqlite(connectionString);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
