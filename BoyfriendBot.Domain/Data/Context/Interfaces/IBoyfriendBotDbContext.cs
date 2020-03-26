using BoyfriendBot.Domain.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Data.Context.Interfaces
{
    public interface IBoyfriendBotDbContext : IDisposable
    {
        DbSet<UserDbo> User { get; set; }
        DbSet<UserSettingsDbo> UserSettings { get; set; }
        DbSet<UserRarityWeightsDbo> RarityWeights { get; set; }
        DbSet<ScheduledMessageDbo> MessageSchedule { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
