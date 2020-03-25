using BoyfriendBot.Domain.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoyfriendBot.Domain.Data.Context.Interfaces
{
    public interface IBoyfriendBotDbContext
    {
        DbSet<UserDbo> User { get; set; }
        DbSet<UserSettingsDbo> UserSettings { get; set; }
        DbSet<ScheduledMessageDbo> MessageSchedule { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
