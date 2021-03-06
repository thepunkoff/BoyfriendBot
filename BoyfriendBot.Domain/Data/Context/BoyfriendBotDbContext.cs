﻿using BoyfriendBot.Domain.AppSettings;
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
        public DbSet<UserRarityWeightsDbo> RarityWeights { get; set; }
        public DbSet<ScheduledMessageDbo> MessageSchedule { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDbo>().HasKey(x => x.UserId);

            modelBuilder.Entity<UserDbo>()
                .HasOne(x => x.UserSettings)
                .WithOne(y => y.User)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<UserDbo>()
                .HasOne(x => x.RarityWeights)
                .WithOne(y => y.User)
                .OnDelete(DeleteBehavior.Cascade);
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if RELEASE
            var path = _appSettings.BoyfriendBotDatabaseAbsolutePath;
#else
            var path = _appSettings.BoyfriendBotDevelopmentDatabaseAbsolutePath;
#endif
            var connectionString = $"Data Source={path}";

            optionsBuilder.UseSqlite(connectionString);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
